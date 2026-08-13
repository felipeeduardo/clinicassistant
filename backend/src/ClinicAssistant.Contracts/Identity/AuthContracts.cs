using System.Text.Json.Serialization;

namespace ClinicAssistant.Contracts.Identity;

public sealed record RegisterClinicRequest(string TenantName, string TenantSlug, string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record AuthResponse(string AccessToken, [property: JsonIgnore] string RefreshToken, DateTimeOffset AccessTokenExpiresAt, UserProfileResponse User);
public sealed record UserProfileResponse(Guid Id, Guid TenantId, string Name, string Email, string Role);
