using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data; // Database context (The IIE, 2026)
using TechMove.Api.Models;
using TechMove.Api.Patterns.Factory;  // Factory pattern implementation (The IIE, 2026)
using TechMove.Api.Patterns.Observer; // Observer pattern implementation (The IIE, 2026)

namespace TechMove.Api.Services
{
    // Manages contract data and implements the Observer subject interface
    // Uses the Factory pattern to create and validate contract types (The IIE, 2026)
    public class ContractService : IContractSubject
    {
        private readonly AppDbContext _db;
        private readonly List<IContractObserver> _observers;

        public ContractService(AppDbContext db, IEnumerable<IContractObserver> observers)
        {
            _db = db;
            // di gives us the starting observers, keep them in a list so we can add or remove at runtime
            _observers = observers.ToList();
        }

        public void RegisterObserver(IContractObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void RemoveObserver(IContractObserver observer)
        {
            _observers.Remove(observer);
        }

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

        // saves the uploaded pdf path against the contract, returns false if the contract is gone
        public async Task<bool> SetAgreementPathAsync(int id, string path)
        {
            var contract = await _db.Contracts.FindAsync(id);
            if (contract == null) return false;

            contract.SignedAgreementPath = path;
            await _db.SaveChangesAsync();
            return true;
        }

        // moves a contract to a new status and tells the observers about it
        // returns null when the contract id does not exist
        public async Task<Contract?> ChangeStatusAsync(int id, ContractStatus status)
        {
            var contract = await _db.Contracts.FindAsync(id);
            if (contract == null) return null;

            // reject status changes that do not make sense (this is the part 2 fix)
            ContractWorkflow.EnsureCanTransition(contract.Status, status);

            contract.Status = status;
            await _db.SaveChangesAsync();
            NotifyObservers(status, contract.Id);
            return contract;
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
