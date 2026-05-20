using TechMove.Web.Patterns.Strategy;
using TechMove.Web.Services;
using Xunit;

namespace TechMove.Tests
{
    public class CurrencyConversionTests
    {
        [Fact]
        public async Task Convert_100USD_ReturnsCorrectZAR()
        {
            var strategy = new MockConversionStrategy(fixedRate: 18.5m);
            var service = new FinancialService(strategy);

            var result = await service.ConvertToZARAsync(100m);

            Assert.Equal(1850.00m, result);
        }

        [Fact]
        public async Task Convert_SetsRateCorrectly_WithDifferentRate()
        {
            var strategy = new MockConversionStrategy(fixedRate: 20.0m);
            var service = new FinancialService(strategy);

            var result = await service.ConvertToZARAsync(50m);

            Assert.Equal(1000.00m, result);
        }

        [Fact]
        public async Task Convert_ZeroAmount_ReturnsZero()
        {
            var strategy = new MockConversionStrategy(fixedRate: 18.5m);
            var service = new FinancialService(strategy);

            var result = await service.ConvertToZARAsync(0m);

            Assert.Equal(0.00m, result);
        }

        [Fact]
        public async Task Convert_NegativeAmount_ReturnsNegativeZAR()
        {
            var strategy = new MockConversionStrategy(fixedRate: 18.5m);
            var service = new FinancialService(strategy);

            var result = await service.ConvertToZARAsync(-10m);

            Assert.Equal(-185.00m, result);
        }

        [Fact]
        public async Task SetStrategy_SwapsStrategy_ReturnsNewRate()
        {
            var strategyA = new MockConversionStrategy(fixedRate: 18.5m);
            var strategyB = new MockConversionStrategy(fixedRate: 19.0m);
            var service = new FinancialService(strategyA);

            service.SetStrategy(strategyB);
            var result = await service.ConvertToZARAsync(100m);

            Assert.Equal(1900.00m, result);
        }

        [Fact]
        public void FinancialService_NullStrategy_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FinancialService(null!));
        }
    }
}
