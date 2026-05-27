using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechMove.Web.Models;

namespace TechMove.Web.ApiClients
{
    // talks to /api/contracts, the date and status filtering is done by the api
    public class ContractApi
    {
        private readonly HttpClient _http;

        public ContractApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Contract>> GetAllAsync(DateTime? from, DateTime? to, string? status)
        {
            var query = new List<string>();
            if (from.HasValue) query.Add($"from={from.Value:yyyy-MM-dd}");
            if (to.HasValue) query.Add($"to={to.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={status}");

            var url = "api/contracts";
            if (query.Count > 0) url += "?" + string.Join("&", query);

            var list = await _http.GetFromJsonAsync<List<ContractResponse>>(url);
            return list?.Select(c => c.ToModel()).ToList() ?? new List<Contract>();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            var resp = await _http.GetAsync($"api/contracts/{id}");
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();

            var contract = await resp.Content.ReadFromJsonAsync<ContractResponse>();
            return contract?.ToModel();
        }

        public async Task<ApiResult<Contract>> CreateAsync(Contract contract)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/contracts", new
                {
                    contract.Title,
                    contract.StartDate,
                    contract.EndDate,
                    contract.ServiceLevel,
                    contract.ClientId
                });

                if (!resp.IsSuccessStatusCode)
                    return ApiResult<Contract>.Fail(await ReadError(resp));

                var created = await resp.Content.ReadFromJsonAsync<ContractResponse>();
                return ApiResult<Contract>.Success(created?.ToModel());
            }
            catch (HttpRequestException)
            {
                return ApiResult<Contract>.Fail("The API could not be reached. Please try again later.");
            }
        }

        public async Task<ApiResult> UpdateAsync(int id, Contract contract)
        {
            try
            {
                var resp = await _http.PutAsJsonAsync($"api/contracts/{id}", new
                {
                    contract.Title,
                    contract.StartDate,
                    contract.EndDate,
                    contract.ServiceLevel,
                    Status = contract.Status.ToString(),
                    contract.ClientId
                });

                return resp.IsSuccessStatusCode ? ApiResult.Success() : ApiResult.Fail(await ReadError(resp));
            }
            catch (HttpRequestException)
            {
                return ApiResult.Fail("The API could not be reached. Please try again later.");
            }
        }

        // approve, hold, expire or reactivate a contract, the api enforces the workflow rules
        public async Task<ApiResult> ChangeStatusAsync(int id, string status)
        {
            try
            {
                var resp = await _http.PatchAsJsonAsync($"api/contracts/{id}/status", new { status });
                return resp.IsSuccessStatusCode ? ApiResult.Success() : ApiResult.Fail(await ReadError(resp));
            }
            catch (HttpRequestException)
            {
                return ApiResult.Fail("The API could not be reached. Please try again later.");
            }
        }

        // posts the signed agreement pdf to the api as multipart form data
        public async Task<ApiResult> UploadAgreementAsync(int id, IFormFile file)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();
                var part = new StreamContent(stream);
                part.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrEmpty(file.ContentType) ? "application/pdf" : file.ContentType);
                content.Add(part, "file", file.FileName);

                var resp = await _http.PostAsync($"api/contracts/{id}/agreement", content);
                return resp.IsSuccessStatusCode ? ApiResult.Success() : ApiResult.Fail(await ReadError(resp));
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
                var resp = await _http.DeleteAsync($"api/contracts/{id}");
                return resp.IsSuccessStatusCode ? ApiResult.Success() : ApiResult.Fail(await ReadError(resp));
            }
            catch (HttpRequestException)
            {
                return ApiResult.Fail("The API could not be reached. Please try again later.");
            }
        }

        private static async Task<string> ReadError(HttpResponseMessage resp)
        {
            var body = (await resp.Content.ReadAsStringAsync()).Trim('"');
            return string.IsNullOrWhiteSpace(body) ? "The request was rejected by the API." : body;
        }
    }
}
