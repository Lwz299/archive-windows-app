using Archive.Domain.Entities;

namespace Archive.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> HasAnyUserAsync();
        Task<User?> GetByUsernameAsync(string username);
        Task<int> AddAsync(User user);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
