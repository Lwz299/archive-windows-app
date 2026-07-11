using System.Net.Http.Json;
using Archive.Contracts.DTOs;

namespace Archive.Desktop.ApiClients
{
    public class CategoryApiClient
    {
        private readonly HttpClient _httpClient;

        public CategoryApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            return _httpClient.GetFromJsonAsync<IEnumerable<CategoryDto>>("api/categories")!;
        }
    }
}
