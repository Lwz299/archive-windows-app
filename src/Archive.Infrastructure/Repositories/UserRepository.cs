using Archive.Domain.Entities;
using Archive.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Archive.Infrastructure.Repositories
{
    public class UserRepository : Archive.Application.Interfaces.IUserRepository
    {
        private readonly ArchiveDbContext _dbContext;

        public UserRepository(ArchiveDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> HasAnyUserAsync()
        {
            return _dbContext.Users.AnyAsync();
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            return _dbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<int> AddAsync(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user.Id;
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return _dbContext.Users.AsNoTracking().ToListAsync().ContinueWith(t => (IEnumerable<User>)t.Result);
        }

        public Task<User?> GetByIdAsync(int id)
        {
            return _dbContext.Users.FindAsync(id).AsTask();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _dbContext.Users.Remove(user);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
