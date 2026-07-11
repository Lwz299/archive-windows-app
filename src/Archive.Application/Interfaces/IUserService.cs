using Archive.Contracts.Responses;

namespace Archive.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetUsersAsync();
        Task<UserResponse?> GetUserByIdAsync(int id);
        Task<bool> DeleteUserAsync(int id);
    }
}
