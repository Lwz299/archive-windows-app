using System.Net.Http.Json;
using Archive.Contracts.Requests;
using Archive.Contracts.Responses;

namespace Archive.Desktop.ApiClients
{
    public class BookApiClient
    {
        private readonly HttpClient _httpClient;

        public BookApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<IEnumerable<BookResponse>> GetBooksAsync(string? search, string? category)
        {
            return _httpClient.GetFromJsonAsync<IEnumerable<BookResponse>>($"api/books?search={Uri.EscapeDataString(search ?? string.Empty)}&category={Uri.EscapeDataString(category ?? string.Empty)}")!;
        }

        public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/books", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BookResponse>() ?? throw new InvalidOperationException("Create book response was empty.");
        }

        public async Task<BookResponse> UpdateBookAsync(UpdateBookRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/books/{request.Id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BookResponse>() ?? throw new InvalidOperationException("Update book response was empty.");
        }

        public async Task DeleteBookAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/books/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
