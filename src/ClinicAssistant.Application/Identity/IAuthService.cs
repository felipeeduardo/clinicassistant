using ClinicAssistant.Contracts.Identity;

namespace ClinicAssistant.Application.Identity;

public interface IAuthService
{
    Task<AuthResponse> RegisterClinicAsync(RegisterClinicRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
    Task<UserProfileResponse> GetCurrentUserAsync(CancellationToken cancellationToken);
}
