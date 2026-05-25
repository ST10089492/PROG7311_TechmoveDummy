using System.Net;
using System.Net.Http.Json;
using TechMove.Web.Models;

namespace TechMove.Web.ApiClients
{
    // talks to /api/clients, the mvc client controller uses this instead of the database
    public class ClientApi
    {
        private readonly HttpClient _http;

        public ClientApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Client>> GetAllAsync()
        {
            var list = await _http.GetFromJsonAsync<List<ClientResponse>>("api/clients");
            return list?.Select(c => c.ToModel()).ToList() ?? new List<Client>();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            var resp = await _http.GetAsync($"api/clients/{id}");
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();

            var client = await resp.Content.ReadFromJsonAsync<ClientResponse>();
            return client?.ToModel();
        }

        public async Task<ApiResult> CreateAsync(Client client)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/clients",
                    new { client.Name, client.ContactDetails, client.Region });
                return await ReadResult(resp);
            }
            catch (HttpRequestException)
            {
                return ApiResult.Fail("The API could not be reached. Please try again later.");
            }
        }

        public async Task<ApiResult> UpdateAsync(int id, Client client)
        {
            try
            {
                var resp = await _http.PutAsJsonAsync($"api/clients/{id}",
                    new { client.Name, client.ContactDetails, client.Region });
                return await ReadResult(resp);
            }
            catch (HttpRequestException)
            {
                return ApiResult.Fail("The API could not be reached. Please try again later.");
            }
        }

        public async Task<ApiResult> DeleteAsync(int id)
        {
            try
            {
                var resp = await _http.DeleteAsync($"api/clients/{id}");
                return await ReadResult(resp);
            }
            catch (HttpRequestException)
            {
                return ApiResult.Fail("The API could not be reached. Please try again later.");
            }
        }

        private static async Task<ApiResult> ReadResult(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode) return ApiResult.Success();
            var body = (await resp.Content.ReadAsStringAsync()).Trim('"');
            return ApiResult.Fail(string.IsNullOrWhiteSpace(body) ? "The request was rejected by the API." : body);
        }
    }
}
