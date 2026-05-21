namespace TechMove.Api.Patterns.Strategy
{
    // Used in unit tests instead of the live API  (Code, 2025)
    public class MockConversionStrategy : ICurrencyConversionStrategy
    {
        private readonly decimal _fixedRate;

        public MockConversionStrategy(decimal fixedRate = 18.5m)
        {
            _fixedRate = fixedRate;
        }

        public Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            return Task.FromResult(Math.Round(amount * _fixedRate, 2));
        }
    }
}
