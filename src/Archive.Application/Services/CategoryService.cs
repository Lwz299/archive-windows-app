using Archive.Application.Interfaces;
using Archive.Contracts.DTOs;

namespace Archive.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IBookRepository _bookRepository;

        public CategoryService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _bookRepository.GetCategoriesAsync();
            return categories.Select(c => new CategoryDto { Name = c });
        }
    }
}
