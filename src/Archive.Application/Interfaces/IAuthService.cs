using Archive.Contracts.Requests;
using Archive.Contracts.Responses;

namespace Archive.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserResponse> RegisterAsync(RegisterRequest request);
        Task<UserResponse> CreateUserAsAdminAsync(RegisterRequest request);
        Task<bool> HasAnyUserAsync();
    }
}
