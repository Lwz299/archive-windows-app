using System.Net.Http.Json;
using Archive.Contracts.Responses;

namespace Archive.Desktop.ApiClients
{
    public class UserApiClient
    {
        private readonly HttpClient _httpClient;

        public UserApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<IEnumerable<UserResponse>> GetUsersAsync()
        {
            return _httpClient.GetFromJsonAsync<IEnumerable<UserResponse>>("api/users")!;
        }

        public Task DeleteUserAsync(int id)
        {
            return _httpClient.DeleteAsync($"api/users/{id}");
        }
    }
}
