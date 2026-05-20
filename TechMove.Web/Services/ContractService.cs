using Microsoft.EntityFrameworkCore;
using TechMove.Web.Data; // Database context (The IIE, 2026)
using TechMove.Web.Models;
using TechMove.Web.Patterns.Factory;  // Factory pattern implementation (The IIE, 2026)
using TechMove.Web.Patterns.Observer; // Observer pattern implementation (The IIE, 2026)

namespace TechMove.Web.Services
{
    // Manages contract data and implements the Observer subject interface
    // Uses the Factory pattern to create and validate contract types (The IIE, 2026)
    public class ContractService : IContractSubject
    {
        private readonly AppDbContext _db;
        private readonly IEnumerable<IContractObserver> _observers;

        public ContractService(AppDbContext db, IEnumerable<IContractObserver> observers)
        {
            _db = db;
            _observers = observers;
        }

        // Observers are registered through DI in Program.cs
        public void RegisterObserver(IContractObserver observer) { }
        public void RemoveObserver(IContractObserver observer) { }

        public void NotifyObservers(ContractStatus newStatus, int contractId)
        {
            foreach (var observer in _observers)
                observer.Update(newStatus, contractId);
        }

        // Picks the right factory based off service level and returns a typed contract object
        public IContract BuildContractType(string serviceLevel)
        {
            ContractFactory factory = serviceLevel switch
            {
                "Premium"       => new SLAContractFactory(),
                "International" => new InternationalContractFactory(),
                _               => new FreightContractFactory()
            };

            var contract = factory.CreateContract(); // Encapsulates object creation logic (Factory pattern) (The IIE, 2026)

            // Pass the service level into the contract so Validate() can check it
            if (contract is FreightContract f)      f.ServiceLevel = serviceLevel;
            if (contract is SLAContract s)          s.ServiceLevel = serviceLevel;
            if (contract is InternationalContract i) i.ServiceLevel = serviceLevel;

            return contract;
        }

        public async Task<List<Contract>> GetAllAsync(DateTime? from, DateTime? to, ContractStatus? status)
        {
            var query = _db.Contracts.Include(c => c.Client).AsQueryable();

            if (from.HasValue)   query = query.Where(c => c.StartDate >= from.Value);
            if (to.HasValue)     query = query.Where(c => c.EndDate <= to.Value);
            if (status.HasValue) query = query.Where(c => c.Status == status.Value);

            return await query.ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
            => await _db.Contracts.Include(c => c.Client)
                                  .Include(c => c.ServiceRequests)
                                  .FirstOrDefaultAsync(c => c.Id == id);

        public async Task CreateAsync(Contract contract)
        {
            // Validate the contract type before saving
            var typed = BuildContractType(contract.ServiceLevel);
            if (!typed.Validate())
                throw new InvalidOperationException("Contract validation failed for the selected service level.");

            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync();
            NotifyObservers(contract.Status, contract.Id);
        }

        public async Task UpdateAsync(Contract contract)
        {
            _db.Contracts.Update(contract);
            await _db.SaveChangesAsync();
            NotifyObservers(contract.Status, contract.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var c = await _db.Contracts.FindAsync(id);
            if (c != null)
            {
                _db.Contracts.Remove(c);
                await _db.SaveChangesAsync();
            }
        }

        // Automatically updates statuses based on start and or end dates
        // OnHold contracts are nott overwritten by this method
        public async Task UpdateStatusesAsync()
        {
            var contracts = await _db.Contracts.ToListAsync();
            var now = DateTime.UtcNow;

            foreach (var c in contracts)
            {
                var newStatus = c.EndDate < now   ? ContractStatus.Expired
                              : c.StartDate > now ? ContractStatus.Draft
                                                  : ContractStatus.Active;

                if (c.Status != ContractStatus.OnHold && c.Status != newStatus)
                {
                    c.Status = newStatus;
                    NotifyObservers(newStatus, c.Id);
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
