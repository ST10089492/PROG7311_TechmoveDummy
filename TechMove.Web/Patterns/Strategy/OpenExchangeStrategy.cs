using System.Text.Json; // Used for parsing JSON responses from API

namespace TechMove.Web.Patterns.Strategy
{
    // Fetches live exchange rates from open.er-api.com 
    public class OpenExchangeStrategy : ICurrencyConversionStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenExchangeStrategy> _logger;

        public OpenExchangeStrategy(HttpClient httpClient, ILogger<OpenExchangeStrategy> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency) // Converts currency using live exchange rates from API
        {
            try
            {
                var url = $"https://open.er-api.com/v6/latest/{fromCurrency.ToUpper()}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var rates = doc.RootElement.GetProperty("rates");
                if (!rates.TryGetProperty(toCurrency.ToUpper(), out var rateElement))
                    throw new InvalidOperationException($"Rate for {toCurrency} not found in response.");

                var rate = rateElement.GetDecimal();
                return Math.Round(amount * rate, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency API call failed. Returning 0.");
                return 0m;
            }
        }
    }
}
