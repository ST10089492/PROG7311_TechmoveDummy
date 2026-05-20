using TechMove.Web.Models;

namespace TechMove.Web.Patterns.Observer
{
    // Writes an audit log entry whenever a contract status changes
    public class ComplianceService : IContractObserver
    {
        private readonly ILogger<ComplianceService> _logger;

        public ComplianceService(ILogger<ComplianceService> logger)
        {
            _logger = logger;
        }

        public void Update(ContractStatus newStatus, int contractId)
        {
            _logger.LogInformation("AUDIT : Contract {ContractId} moved to {Status} at {Time}.", contractId, newStatus, DateTime.UtcNow);
        }
    }
}
