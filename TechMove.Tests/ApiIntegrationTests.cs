using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace TechMove.Tests
{
    // these call the running api over http and check the responses, which is what the
    // automated integration testing part of the brief asks for (The IIE, 2026)
    public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // small shapes just for reading the json back in the tests
        private class TokenResponse { public string Token { get; set; } = string.Empty; }
        private class IdResponse { public int Id { get; set; } }
        private class ContractRead { public int Id { get; set; } public string Title { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; }

        private async Task<HttpClient> CreateLoggedInClientAsync()
        {
            var client = _factory.CreateClient();
            var login = await client.PostAsJsonAsync("api/auth/login", new { username = "admin", password = "Admin123!" });
            login.EnsureSuccessStatusCode();
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.Token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async Task<int> CreateClientAsync(HttpClient client)
        {
            var resp = await client.PostAsJsonAsync("api/clients",
                new { name = "Test Client", contactDetails = "test@test.com", region = "Gauteng" });
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        }

        [Fact]
        public async Task GetContracts_ReturnsOk_AndBodyIsNotNull()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("api/contracts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(body));
        }

        [Fact]
        public async Task GetClients_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("api/clients");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithSeededAdmin_ReturnsToken()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/auth/login",
                new { username = "admin", password = "Admin123!" });

            response.EnsureSuccessStatusCode();
            var token = (await response.Content.ReadFromJsonAsync<TokenResponse>())!.Token;
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task CreateContract_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/contracts", new
            {
                title = "No Auth",
                startDate = DateTime.UtcNow,
                endDate = DateTime.UtcNow.AddDays(30),
                serviceLevel = "Standard",
                clientId = 1
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateThenRead_Contract_ReturnsSameData()
        {
            var client = await CreateLoggedInClientAsync();
            var clientId = await CreateClientAsync(client);

            // create
            var create = await client.PostAsJsonAsync("api/contracts", new
            {
                title = "Integration Contract",
                startDate = DateTime.UtcNow,
                endDate = DateTime.UtcNow.AddDays(30),
                serviceLevel = "Standard",
                clientId
            });
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            var created = await create.Content.ReadFromJsonAsync<ContractRead>();

            // read it back
            var read = await client.GetAsync($"api/contracts/{created!.Id}");
            Assert.Equal(HttpStatusCode.OK, read.StatusCode);
            var fetched = await read.Content.ReadFromJsonAsync<ContractRead>();

            Assert.Equal("Integration Contract", fetched!.Title);
        }

        [Fact]
        public async Task GetContract_UnknownId_ReturnsNotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("api/contracts/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateServiceRequest_OnExpiredContract_ReturnsBadRequest()
        {
            var client = await CreateLoggedInClientAsync();
            var clientId = await CreateClientAsync(client);

            var create = await client.PostAsJsonAsync("api/contracts", new
            {
                title = "Soon Expired",
                startDate = DateTime.UtcNow,
                endDate = DateTime.UtcNow.AddDays(30),
                serviceLevel = "Standard",
                clientId
            });
            var contract = await create.Content.ReadFromJsonAsync<ContractRead>();

            // move it to Expired, then a service request must be refused
            await client.PatchAsJsonAsync($"api/contracts/{contract!.Id}/status", new { status = "Expired" });

            var srResponse = await client.PostAsJsonAsync("api/servicerequests", new
            {
                description = "Should be blocked",
                costUSD = 100m,
                contractId = contract.Id
            });

            Assert.Equal(HttpStatusCode.BadRequest, srResponse.StatusCode);
        }

        [Fact]
        public async Task PatchContractStatus_IllegalMove_ReturnsBadRequest()
        {
            var client = await CreateLoggedInClientAsync();
            var clientId = await CreateClientAsync(client);

            var create = await client.PostAsJsonAsync("api/contracts", new
            {
                title = "Status Rules",
                startDate = DateTime.UtcNow,
                endDate = DateTime.UtcNow.AddDays(30),
                serviceLevel = "Standard",
                clientId
            });
            var contract = await create.Content.ReadFromJsonAsync<ContractRead>();

            // Expired then back to OnHold is not a legal move
            await client.PatchAsJsonAsync($"api/contracts/{contract!.Id}/status", new { status = "Expired" });
            var illegal = await client.PatchAsJsonAsync($"api/contracts/{contract.Id}/status", new { status = "OnHold" });

            Assert.Equal(HttpStatusCode.BadRequest, illegal.StatusCode);
        }
    }
}
