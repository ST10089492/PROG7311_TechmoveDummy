using System.Net.Http.Json;

namespace TechMove.Web.ApiClients
{
    // calls the api login endpoint and gives back the jwt, or null when the details are wrong
    public class AuthApi
    {
        private readonly HttpClient _http;

        public AuthApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
                if (!resp.IsSuccessStatusCode) return null;

                var result = await resp.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.Token;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
