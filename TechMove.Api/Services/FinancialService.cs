using TechMove.Api.Patterns.Strategy;

namespace TechMove.Api.Services
{
    public class FinancialService // Manages and executes different currency conversion strategies (The IIE, 2026)
    {
        private ICurrencyConversionStrategy _strategy; // Holds the current strategy implementation (The IIE, 2026)

        public FinancialService(ICurrencyConversionStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public void SetStrategy(ICurrencyConversionStrategy strategy)
        {
            _strategy = strategy;
        }

        public async Task<decimal> ConvertToZARAsync(decimal amountUSD)  // Converts USD to ZAR using the selected strategy
        {
            return await _strategy.ConvertAsync(amountUSD, "USD", "ZAR");
        }
    }
}
