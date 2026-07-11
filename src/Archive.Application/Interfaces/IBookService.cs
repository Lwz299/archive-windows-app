using Archive.Contracts.Requests;
using Archive.Contracts.Responses;

namespace Archive.Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponse>> GetBooksAsync(string? search, string? category);
        Task<BookResponse?> GetBookByIdAsync(int id);
        Task<BookResponse> CreateBookAsync(CreateBookRequest request);
        Task<BookResponse> UpdateBookAsync(UpdateBookRequest request);
        Task<bool> DeleteBookAsync(int id);
        Task<IEnumerable<string>> GetCategoriesAsync();
    }
}
