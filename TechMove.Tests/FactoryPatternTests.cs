using Microsoft.EntityFrameworkCore;
using TechMove.Web.Patterns.Factory;
using TechMove.Web.Services;
using Xunit;

namespace TechMove.Tests
{
    public class FactoryPatternTests
    {
        [Fact]
        public void BuildContractType_Standard_ReturnsFreightContract()
        {
            var service = CreateContractService();
            var contract = service.BuildContractType("Standard");
            Assert.IsType<FreightContract>(contract);
        }

        [Fact]
        public void BuildContractType_Premium_ReturnsSLAContract()
        {
            var service = CreateContractService();
            var contract = service.BuildContractType("Premium");
            Assert.IsType<SLAContract>(contract);
        }

        [Fact]
        public void BuildContractType_International_ReturnsInternationalContract()
        {
            var service = CreateContractService();
            var contract = service.BuildContractType("International");
            Assert.IsType<InternationalContract>(contract);
        }

        [Fact]
        public void BuildContractType_Unknown_DefaultsToFreightContract()
        {
            var service = CreateContractService();
            var contract = service.BuildContractType("SomethingUnknown");
            Assert.IsType<FreightContract>(contract);
        }

        [Fact]
        public void FreightContract_Standard_ValidatesTrue()
        {
            var contract = new FreightContract { ServiceLevel = "Standard" };
            Assert.True(contract.Validate());
        }

        [Fact]
        public void FreightContract_Premium_ValidatesFalse()
        {
            var contract = new FreightContract { ServiceLevel = "Premium" };
            Assert.False(contract.Validate());
        }

        [Fact]
        public void SLAContract_Premium_ValidatesTrue()
        {
            var contract = new SLAContract { ServiceLevel = "Premium" };
            Assert.True(contract.Validate());
        }

        [Fact]
        public void SLAContract_Standard_ValidatesFalse()
        {
            var contract = new SLAContract { ServiceLevel = "Standard" };
            Assert.False(contract.Validate());
        }

        [Fact]
        public void InternationalContract_International_ValidatesTrue()
        {
            var contract = new InternationalContract { ServiceLevel = "International" };
            Assert.True(contract.Validate());
        }

        [Fact]
        public void InternationalContract_Standard_ValidatesFalse()
        {
            var contract = new InternationalContract { ServiceLevel = "Standard" };
            Assert.False(contract.Validate());
        }

        [Fact]
        public void FreightContract_GetContractType_ReturnsFreight()
        {
            var c = new FreightContract();
            Assert.Equal("Freight", c.GetContractType());
        }

        [Fact]
        public void SLAContract_GetContractType_ReturnsSLA()
        {
            var c = new SLAContract();
            Assert.Equal("SLA", c.GetContractType());
        }

        [Fact]
        public void InternationalContract_GetContractType_ReturnsInternational()
        {
            var c = new InternationalContract();
            Assert.Equal("International", c.GetContractType());
        }

        private ContractService CreateContractService()
        {
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TechMove.Web.Data.AppDbContext>()
                .UseInMemoryDatabase("factory_test_" + Guid.NewGuid())
                .Options;
            var db = new TechMove.Web.Data.AppDbContext(options);
            return new ContractService(db, Enumerable.Empty<TechMove.Web.Patterns.Observer.IContractObserver>());
        }
    }
}
