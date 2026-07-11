using Archive.Application.Interfaces;
using Archive.Contracts.Requests;
using Archive.Contracts.Responses;
using Archive.Domain.Entities;

namespace Archive.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<BookResponse>> GetBooksAsync(string? search, string? category)
        {
            var books = await _bookRepository.GetAllAsync(search, category);
            return books.Select(MapBookResponse);
        }

        public async Task<BookResponse?> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            return book == null ? null : MapBookResponse(book);
        }

        public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
        {
            var book = new Book
            {
                Title = request.Title.Trim(),
                Author = request.Author.Trim(),
                Category = request.Category.Trim(),
                Publisher = request.Publisher.Trim(),
                Year = request.Year,
                Language = request.Language.Trim(),
                Notes = request.Notes.Trim(),
                CoverUrl = request.CoverUrl ?? request.CoverPath,
                FileUrl = request.FileUrl,
                FilePath = request.FilePath,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
            };

            book.Id = await _bookRepository.AddAsync(book);
            return MapBookResponse(book);
        }

        public async Task<BookResponse> UpdateBookAsync(UpdateBookRequest request)
        {
            var existing = await _bookRepository.GetByIdAsync(request.Id)
                ?? throw new InvalidOperationException("الكتاب غير موجود");

            existing.Title = request.Title.Trim();
            existing.Author = request.Author.Trim();
            existing.Category = request.Category.Trim();
            existing.Publisher = request.Publisher.Trim();
            existing.Year = request.Year;
            existing.Language = request.Language.Trim();
            existing.Notes = request.Notes.Trim();

            var cover = request.CoverUrl ?? request.CoverPath;
            if (!string.IsNullOrWhiteSpace(cover))
            {
                existing.CoverUrl = cover;
            }

            if (!string.IsNullOrWhiteSpace(request.FileUrl))
            {
                existing.FileUrl = request.FileUrl;
            }

            if (!string.IsNullOrWhiteSpace(request.FilePath))
            {
                existing.FilePath = request.FilePath;
            }

            await _bookRepository.UpdateAsync(existing);
            return MapBookResponse(existing);
        }

        public Task<bool> DeleteBookAsync(int id)
        {
            return _bookRepository.DeleteAsync(id);
        }

        public Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return _bookRepository.GetCategoriesAsync();
        }

        private static BookResponse MapBookResponse(Book book)
        {
            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Category = book.Category,
                Publisher = book.Publisher,
                Year = book.Year,
                Language = book.Language,
                Notes = book.Notes,
                CoverUrl = book.CoverUrl,
                FilePath = book.FilePath,
                FileUrl = book.FileUrl,
                CreatedAt = book.CreatedAt
            };
        }
    }
}
