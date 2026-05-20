using TechMove.Web.Models;

namespace TechMove.Web.Patterns.Observer
{
    // Stops invoice generation when a contract expires 
    public class BillingService : IContractObserver //(The IIE, 2026)
    {
        private readonly ILogger<BillingService> _logger;

        public BillingService(ILogger<BillingService> logger)
        {
            _logger = logger;
        }

        public void Update(ContractStatus newStatus, int contractId)
        {
            if (newStatus == ContractStatus.Expired || newStatus == ContractStatus.OnHold)
            {
                _logger.LogWarning("Contract {ContractId} is {Status} : invoice generation halted.", contractId, newStatus);
            }
            else
            {
                _logger.LogInformation("Contract {ContractId} is {Status} : billing active.", contractId, newStatus);
            }
        }
    }
}
