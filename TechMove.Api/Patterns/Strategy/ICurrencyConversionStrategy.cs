namespace TechMove.Api.Patterns.Strategy
{
    // Strategy interface for currency conversion (The IIE, 2026)
    public interface ICurrencyConversionStrategy
    {
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
