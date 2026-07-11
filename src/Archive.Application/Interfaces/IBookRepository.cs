using Archive.Domain.Entities;

namespace Archive.Application.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync(string? search, string? category);
        Task<Book?> GetByIdAsync(int id);
        Task<int> AddAsync(Book book);
        Task<bool> UpdateAsync(Book book);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<string>> GetCategoriesAsync();
    }
}
