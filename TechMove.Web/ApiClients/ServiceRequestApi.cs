using System.Net;
using System.Net.Http.Json;
using TechMove.Web.Models;

namespace TechMove.Web.ApiClients
{
    // talks to /api/servicerequests, the workflow check and currency conversion happen on the api side
    public class ServiceRequestApi
    {
        private readonly HttpClient _http;

        public ServiceRequestApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ServiceRequest>> GetAllAsync()
        {
            var list = await _http.GetFromJsonAsync<List<ServiceRequestResponse>>("api/servicerequests");
            return list?.Select(s => s.ToModel()).ToList() ?? new List<ServiceRequest>();
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            var resp = await _http.GetAsync($"api/servicerequests/{id}");
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();

            var sr = await resp.Content.ReadFromJsonAsync<ServiceRequestResponse>();
            return sr?.ToModel();
        }

        public async Task<ApiResult<ServiceRequest>> CreateAsync(ServiceRequest request)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/servicerequests", new
                {
                    request.Description,
                    request.CostUSD,
                    request.ContractId
                });

                if (!resp.IsSuccessStatusCode)
                {
                    var body = (await resp.Content.ReadAsStringAsync()).Trim('"');
                    return ApiResult<ServiceRequest>.Fail(string.IsNullOrWhiteSpace(body)
                        ? "The request was rejected by the API." : body);
                }

                var created = await resp.Content.ReadFromJsonAsync<ServiceRequestResponse>();
                return ApiResult<ServiceRequest>.Success(created?.ToModel());
            }
            catch (HttpRequestException)
            {
                return ApiResult<ServiceRequest>.Fail("The API could not be reached. Please try again later.");
            }
        }

        // move the request along its workflow, the api blocks any step that is not allowed
        public async Task<ApiResult> ChangeStatusAsync(int id, string status)
        {
            try
            {
                var resp = await _http.PatchAsJsonAsync($"api/servicerequests/{id}/status", new { status });
                if (resp.IsSuccessStatusCode) return ApiResult.Success();
                var body = (await resp.Content.ReadAsStringAsync()).Trim('"');
                return ApiResult.Fail(string.IsNullOrWhiteSpace(body) ? "The request was rejected by the API." : body);
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
                var resp = await _http.DeleteAsync($"api/servicerequests/{id}");
                if (resp.IsSuccessStatusCode) return ApiResult.Success();
                var body = (await resp.Content.ReadAsStringAsync()).Trim('"');
                return ApiResult.Fail(string.IsNullOrWhiteSpace(body) ? "The request was rejected by the API." : body);
            }
            catch (HttpRequestException)
            {
                return ApiResult.Fail("The API could not be reached. Please try again later.");
            }
        }
    }
}
