using Microsoft.EntityFrameworkCore;
using TechMove.Web.Data;
using TechMove.Web.Models;
using TechMove.Web.Services;
using Xunit;

namespace TechMove.Tests
{
    public class ContractStatusTests
    {
        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private ContractService CreateService(AppDbContext db)
            => new ContractService(db, Enumerable.Empty<TechMove.Web.Patterns.Observer.IContractObserver>());

        private Contract MakeContract(DateTime start, DateTime end, ContractStatus status) => new Contract
        {
            Title = "Test", ServiceLevel = "Standard", ClientId = 1,
            StartDate = start, EndDate = end, Status = status
        };

        [Fact]
        public async Task UpdateStatusesAsync_PastEndDate_SetsExpired()
        {
            var db = CreateDb("expire_past");
            db.Contracts.Add(MakeContract(
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow.AddDays(-1),   // ended yesterday
                ContractStatus.Active));
            await db.SaveChangesAsync();

            await CreateService(db).UpdateStatusesAsync();

            var contract = await db.Contracts.FirstAsync();
            Assert.Equal(ContractStatus.Expired, contract.Status);
        }

        [Fact]
        public async Task UpdateStatusesAsync_FutureStartDate_SetsDraft()
        {
            var db = CreateDb("draft_future");
            db.Contracts.Add(MakeContract(
                DateTime.UtcNow.AddDays(5),    // starts in 5 days
                DateTime.UtcNow.AddDays(35),
                ContractStatus.Active));
            await db.SaveChangesAsync();

            await CreateService(db).UpdateStatusesAsync();

            var contract = await db.Contracts.FirstAsync();
            Assert.Equal(ContractStatus.Draft, contract.Status);
        }

        [Fact]
        public async Task UpdateStatusesAsync_CurrentDates_SetsActive()
        {
            var db = CreateDb("active_current");
            db.Contracts.Add(MakeContract(
                DateTime.UtcNow.AddDays(-5),
                DateTime.UtcNow.AddDays(25),
                ContractStatus.Draft));
            await db.SaveChangesAsync();

            await CreateService(db).UpdateStatusesAsync();

            var contract = await db.Contracts.FirstAsync();
            Assert.Equal(ContractStatus.Active, contract.Status);
        }

        [Fact]
        public async Task UpdateStatusesAsync_OnHoldContract_StatusPreserved()
        {
            var db = CreateDb("onhold_preserved");
            db.Contracts.Add(MakeContract(
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddDays(-1),   // would auto-expire
                ContractStatus.OnHold));
            await db.SaveChangesAsync();

            await CreateService(db).UpdateStatusesAsync();

            var contract = await db.Contracts.FirstAsync();
            Assert.Equal(ContractStatus.OnHold, contract.Status); // must not be changed to Expired
        }
    }
}
