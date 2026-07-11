using Archive.Application.Interfaces;
using Archive.Contracts.Responses;

namespace Archive.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserResponse>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public Task<bool> DeleteUserAsync(int id)
        {
            return _userRepository.DeleteAsync(id);
        }
    }
}
