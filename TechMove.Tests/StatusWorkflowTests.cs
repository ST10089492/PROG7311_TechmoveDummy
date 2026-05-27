using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Models;
using TechMove.Api.Patterns.Observer;
using TechMove.Api.Patterns.Strategy;
using TechMove.Api.Services;
using Xunit;

namespace TechMove.Tests
{
    // covers the status workflow that was missing in part 2, both the rules on their own
    // and the service methods that use them
    public class StatusWorkflowTests
    {
        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private ContractService CreateContractService(AppDbContext db)
            => new ContractService(db, Enumerable.Empty<IContractObserver>());

        private ServiceRequestService CreateSrService(AppDbContext db)
            => new ServiceRequestService(db, new FinancialService(new MockConversionStrategy()));

        // --- contract rules ---

        [Theory]
        [InlineData(ContractStatus.Draft, ContractStatus.Active)]
        [InlineData(ContractStatus.Draft, ContractStatus.OnHold)]
        [InlineData(ContractStatus.Draft, ContractStatus.Expired)]
        [InlineData(ContractStatus.Active, ContractStatus.OnHold)]
        [InlineData(ContractStatus.Active, ContractStatus.Expired)]
        [InlineData(ContractStatus.OnHold, ContractStatus.Active)]
        [InlineData(ContractStatus.OnHold, ContractStatus.Expired)]
        [InlineData(ContractStatus.Expired, ContractStatus.Active)]
        public void ContractWorkflow_AllowedMoves_AreValid(ContractStatus from, ContractStatus to)
        {
            Assert.True(ContractWorkflow.CanTransition(from, to));
        }

        [Theory]
        [InlineData(ContractStatus.Active, ContractStatus.Draft)]
        [InlineData(ContractStatus.Expired, ContractStatus.OnHold)]
        [InlineData(ContractStatus.Expired, ContractStatus.Draft)]
        [InlineData(ContractStatus.OnHold, ContractStatus.Draft)]
        public void ContractWorkflow_BlockedMoves_AreRejected(ContractStatus from, ContractStatus to)
        {
            Assert.False(ContractWorkflow.CanTransition(from, to));
            Assert.Throws<InvalidOperationException>(() => ContractWorkflow.EnsureCanTransition(from, to));
        }

        [Fact]
        public void ContractWorkflow_SameStatus_IsRejected()
        {
            Assert.Throws<InvalidOperationException>(
                () => ContractWorkflow.EnsureCanTransition(ContractStatus.Active, ContractStatus.Active));
        }

        // --- service request rules ---

        [Theory]
        [InlineData("Pending", "InProgress")]
        [InlineData("Pending", "Cancelled")]
        [InlineData("InProgress", "Completed")]
        [InlineData("InProgress", "Cancelled")]
        public void ServiceRequestWorkflow_AllowedMoves_AreValid(string from, string to)
        {
            Assert.True(ServiceRequestWorkflow.CanTransition(from, to));
        }

        [Theory]
        [InlineData("Pending", "Completed")]
        [InlineData("Completed", "Pending")]
        [InlineData("Completed", "InProgress")]
        [InlineData("Cancelled", "InProgress")]
        public void ServiceRequestWorkflow_BlockedMoves_AreRejected(string from, string to)
        {
            Assert.False(ServiceRequestWorkflow.CanTransition(from, to));
            Assert.Throws<InvalidOperationException>(() => ServiceRequestWorkflow.EnsureCanTransition(from, to));
        }

        [Fact]
        public void ServiceRequestWorkflow_UnknownStatus_IsRejected()
        {
            Assert.False(ServiceRequestWorkflow.IsKnownStatus("Shipped"));
            Assert.Throws<InvalidOperationException>(
                () => ServiceRequestWorkflow.EnsureCanTransition("Pending", "Shipped"));
        }

        // --- service methods that use the rules ---

        [Fact]
        public async Task ContractService_ChangeStatus_AllowedMove_Succeeds()
        {
            var db = CreateDb("contract_change_ok");
            db.Contracts.Add(new Contract
            {
                Title = "Test", ServiceLevel = "Standard", ClientId = 1,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30),
                Status = ContractStatus.Draft
            });
            await db.SaveChangesAsync();
            var id = (await db.Contracts.FirstAsync()).Id;

            await CreateContractService(db).ChangeStatusAsync(id, ContractStatus.Active);

            Assert.Equal(ContractStatus.Active, (await db.Contracts.FindAsync(id))!.Status);
        }

        [Fact]
        public async Task ContractService_ChangeStatus_BlockedMove_Throws()
        {
            var db = CreateDb("contract_change_blocked");
            db.Contracts.Add(new Contract
            {
                Title = "Test", ServiceLevel = "Standard", ClientId = 1,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30),
                Status = ContractStatus.Active
            });
            await db.SaveChangesAsync();
            var id = (await db.Contracts.FirstAsync()).Id;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateContractService(db).ChangeStatusAsync(id, ContractStatus.Draft));
        }

        [Fact]
        public async Task ServiceRequestService_ChangeStatus_AllowedMove_Succeeds()
        {
            var db = CreateDb("sr_change_ok");
            db.ServiceRequests.Add(new ServiceRequest { Description = "Test", ContractId = 1, Status = "Pending" });
            await db.SaveChangesAsync();
            var id = (await db.ServiceRequests.FirstAsync()).Id;

            await CreateSrService(db).ChangeStatusAsync(id, "InProgress");

            Assert.Equal("InProgress", (await db.ServiceRequests.FindAsync(id))!.Status);
        }

        [Fact]
        public async Task ServiceRequestService_ChangeStatus_BlockedMove_Throws()
        {
            var db = CreateDb("sr_change_blocked");
            db.ServiceRequests.Add(new ServiceRequest { Description = "Test", ContractId = 1, Status = "Pending" });
            await db.SaveChangesAsync();
            var id = (await db.ServiceRequests.FirstAsync()).Id;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateSrService(db).ChangeStatusAsync(id, "Completed"));
        }
    }
}
