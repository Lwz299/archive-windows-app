using Archive.Contracts.DTOs;

namespace Archive.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    }
}
