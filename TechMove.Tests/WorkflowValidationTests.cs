using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Models;
using TechMove.Api.Patterns.Strategy;
using TechMove.Api.Services;
using Xunit;

namespace TechMove.Tests
{
    public class WorkflowValidationTests
    {
        private AppDbContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        private ServiceRequestService CreateService(AppDbContext db)
        {
            var financial = new FinancialService(new MockConversionStrategy(18.5m));
            return new ServiceRequestService(db, financial);
        }

        private Contract MakeContract(ContractStatus status) => new Contract
        {
            Id = 1,
            Title = "Test Contract",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            Status = status,
            ServiceLevel = "Standard",
            ClientId = 1
        };

        [Fact]
        public async Task CreateServiceRequest_ExpiredContract_ThrowsInvalidOperationException()
        {
            var db = CreateInMemoryDb("expired_test");
            db.Contracts.Add(MakeContract(ContractStatus.Expired));
            await db.SaveChangesAsync();

            var service = CreateService(db);
            var request = new ServiceRequest { ContractId = 1, Description = "Test", CostUSD = 100m };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateServiceRequest_OnHoldContract_ThrowsInvalidOperationException()
        {
            var db = CreateInMemoryDb("onhold_test");
            db.Contracts.Add(MakeContract(ContractStatus.OnHold));
            await db.SaveChangesAsync();

            var service = CreateService(db);
            var request = new ServiceRequest { ContractId = 1, Description = "Test", CostUSD = 100m };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateServiceRequest_ActiveContract_Succeeds()
        {
            var db = CreateInMemoryDb("active_test");
            db.Contracts.Add(MakeContract(ContractStatus.Active));
            await db.SaveChangesAsync();

            var service = CreateService(db);
            var request = new ServiceRequest { ContractId = 1, Description = "Delivery", CostUSD = 200m };

            await service.CreateAsync(request);

            Assert.Equal(1, await db.ServiceRequests.CountAsync());
        }

        [Fact]
        public async Task CreateServiceRequest_ActiveContract_CostZARIsCalculated()
        {
            var db = CreateInMemoryDb("zar_calc_test");
            db.Contracts.Add(MakeContract(ContractStatus.Active));
            await db.SaveChangesAsync();

            var service = CreateService(db);
            var request = new ServiceRequest { ContractId = 1, Description = "Delivery", CostUSD = 100m };

            await service.CreateAsync(request);

            var saved = await db.ServiceRequests.FirstAsync();
            Assert.Equal(1850.00m, saved.CostZAR);
        }

        [Fact]
        public async Task CreateServiceRequest_ContractNotFound_ThrowsInvalidOperationException()
        {
            var db = CreateInMemoryDb("notfound_test");
            var service = CreateService(db);
            var request = new ServiceRequest { ContractId = 999, Description = "Test", CostUSD = 50m };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
        }
    }
}
