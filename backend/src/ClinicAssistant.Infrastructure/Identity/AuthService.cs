using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Contracts.Identity;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClinicAssistant.Infrastructure.Identity;

public sealed class AuthService(ClinicAssistantDbContext dbContext, ITenantContext tenantContext, IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<AuthResponse> RegisterClinicAsync(RegisterClinicRequest request, CancellationToken cancellationToken)
    {
        var slug = NormalizeSlug(request.TenantSlug);
        var email = NormalizeEmail(request.Email);
        if (await dbContext.Tenants.AnyAsync(tenant => tenant.Slug == slug, cancellationToken)) throw new InvalidOperationException("This tenant slug is already in use.");
        if (await dbContext.Users.IgnoreQueryFilters().AnyAsync(user => user.Email == email, cancellationToken)) throw new InvalidOperationException("This email is already in use.");

        var tenant = new Tenant(request.TenantName.Trim(), slug);
        var user = new User(tenant.Id, request.Name.Trim(), email, PasswordHasher.Hash(request.Password), UserRole.ClinicAdmin);
        dbContext.AddRange(tenant, user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users.IgnoreQueryFilters().Include(candidate => candidate.RefreshTokens)
            .SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);
        if (user is null || user.Status != UserStatus.Active || !PasswordHasher.Verify(request.Password, user.PasswordHash)) throw new UnauthorizedAccessException("Invalid email or password.");
        user.RecordLogin();
        await dbContext.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = HashRefreshToken(request.RefreshToken);
        var token = await dbContext.RefreshTokens.IgnoreQueryFilters().Include(candidate => candidate.User)
            .SingleOrDefaultAsync(candidate => candidate.TokenHash == tokenHash, cancellationToken);
        if (token?.RevokedAt is not null) OperationalTelemetry.RefreshTokenReuseDetected.Add(1);
        if (token is null || !token.IsActive(DateTimeOffset.UtcNow) || token.User.Status != UserStatus.Active) throw new UnauthorizedAccessException("Invalid refresh token.");

        var replacement = CreateRefreshToken(token.User);
        token.Revoke(replacement.Entity.TokenHash);
        dbContext.RefreshTokens.Add(replacement.Entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        OperationalTelemetry.RefreshTokenRotations.Add(1);
        return CreateAuthResponse(token.User, replacement);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = HashRefreshToken(request.RefreshToken);
        var token = await dbContext.RefreshTokens.IgnoreQueryFilters().SingleOrDefaultAsync(candidate => candidate.TokenHash == tokenHash, cancellationToken);
        if (token is not null && token.IsActive(DateTimeOffset.UtcNow))
        {
            token.Revoke();
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserProfileResponse> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = tenantContext.UserId ?? throw new UnauthorizedAccessException("An authenticated user is required.");
        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("The authenticated user is unavailable.");
        return ToProfile(user);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var refreshToken = CreateRefreshToken(user);
        dbContext.RefreshTokens.Add(refreshToken.Entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreateAuthResponse(user, refreshToken);
    }

    private AuthResponse CreateAuthResponse(User user, IssuedRefreshToken refreshToken)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_jwt.AccessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()), new Claim("tenant_id", user.TenantId.ToString())
        };
        var jwt = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, now.UtcDateTime, expiresAt.UtcDateTime, new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(jwt), refreshToken.RawValue, expiresAt, ToProfile(user));
    }

    private IssuedRefreshToken CreateRefreshToken(User user)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken(user.TenantId, user.Id, HashRefreshToken(rawToken), DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenDays));
        return new IssuedRefreshToken(entity, rawToken);
    }

    private static UserProfileResponse ToProfile(User user) => new(user.Id, user.TenantId, user.Name, user.Email, user.Role.ToString());
    private static string HashRefreshToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private static string NormalizeSlug(string slug) => slug.Trim().ToLowerInvariant();
    private sealed record IssuedRefreshToken(RefreshToken Entity, string RawValue);
}
