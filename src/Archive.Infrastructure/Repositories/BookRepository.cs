using Archive.Domain.Entities;
using Archive.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Archive.Infrastructure.Repositories
{
    public class BookRepository : Archive.Application.Interfaces.IBookRepository
    {
        private readonly ArchiveDbContext _dbContext;

        public BookRepository(ArchiveDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static void EnsureSchema(ArchiveDbContext dbContext)
        {
            dbContext.Database.EnsureCreated();
            if (dbContext.Database.IsSqlite())
            {
                try
                {
                    dbContext.Database.ExecuteSqlRaw(
                        "ALTER TABLE Books ADD COLUMN FilePath TEXT NULL");
                }
                catch
                {
                    // Column already exists
                }
            }
        }

        public async Task<IEnumerable<Book>> GetAllAsync(string? search, string? category)
        {
            var query = _dbContext.Books.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search) || b.Notes.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category == category);
            }

            return await query.OrderByDescending(b => b.Id).ToListAsync();
        }

        public Task<Book?> GetByIdAsync(int id)
        {
            return _dbContext.Books.FindAsync(id).AsTask();
        }

        public async Task<int> AddAsync(Book book)
        {
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();
            return book.Id;
        }

        public async Task<bool> UpdateAsync(Book book)
        {
            _dbContext.Books.Update(book);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _dbContext.Books.FindAsync(id);
            if (existing == null)
            {
                return false;
            }

            _dbContext.Books.Remove(existing);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _dbContext.Books
                .AsNoTracking()
                .Where(b => !string.IsNullOrEmpty(b.Category))
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}
