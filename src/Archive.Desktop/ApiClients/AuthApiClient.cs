using System.Net.Http.Json;
using Archive.Contracts.Requests;
using Archive.Contracts.Responses;

namespace Archive.Desktop.ApiClients
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<bool> CheckSetupAsync()
        {
            return _httpClient.GetFromJsonAsync<bool>("api/auth/setup")!;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponse>() ?? throw new InvalidOperationException("Login response was empty.");
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserResponse>() ?? throw new InvalidOperationException("Register response was empty.");
        }
    }
}
