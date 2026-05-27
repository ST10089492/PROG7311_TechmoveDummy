using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Models;

namespace TechMove.Api.Services
{
    // Handles service request business logic
    public class ServiceRequestService // Shows separation of concerns in enterprise applications (The IIE, 2026)
    {
        private readonly AppDbContext _db; // Provides access to database entities (The IIE, 2026)
        private readonly FinancialService _financialService;

        public ServiceRequestService(AppDbContext db, FinancialService financialService)
        {
            _db = db;
            _financialService = financialService;
        }

        public async Task<List<ServiceRequest>> GetAllAsync()
            => await _db.ServiceRequests.Include(sr => sr.Contract).ToListAsync();

        public async Task<List<ServiceRequest>> GetByContractAsync(int contractId)
            => await _db.ServiceRequests.Where(sr => sr.ContractId == contractId).ToListAsync();

        public async Task<ServiceRequest?> GetByIdAsync(int id)
            => await _db.ServiceRequests.Include(sr => sr.Contract).FirstOrDefaultAsync(sr => sr.Id == id);

        public async Task CreateAsync(ServiceRequest request)
        {
            var contract = await _db.Contracts.FindAsync(request.ContractId)
                ?? throw new InvalidOperationException("Contract not found.");

            if (contract.Status == ContractStatus.Expired)
                throw new InvalidOperationException("A service request cannot be created for an Expired contract.");

            if (contract.Status == ContractStatus.OnHold)
                throw new InvalidOperationException("A service request cannot be created for a contract that is On Hold.");

            // Convert the USD cost to ZAR using the currency API
            request.CostZAR = await _financialService.ConvertToZARAsync(request.CostUSD);
            request.CreatedOn = DateTime.UtcNow;

            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ServiceRequest request) // Updates an existing service request
        {
            _db.ServiceRequests.Update(request);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id) // Deletes a service request from the database
        {
            var sr = await _db.ServiceRequests.FindAsync(id);
            if (sr != null)
            {
                _db.ServiceRequests.Remove(sr);
                await _db.SaveChangesAsync();
            }
        }

        // moves a request along its workflow (Pending, InProgress, Completed)
        // returns null when the id does not exist
        public async Task<ServiceRequest?> ChangeStatusAsync(int id, string status)
        {
            var sr = await _db.ServiceRequests.FindAsync(id);
            if (sr == null) return null;

            // only allow the steps the workflow permits, no jumping straight to Completed etc
            ServiceRequestWorkflow.EnsureCanTransition(sr.Status, status);

            sr.Status = status;
            await _db.SaveChangesAsync();
            return sr;
        }
    }
}
