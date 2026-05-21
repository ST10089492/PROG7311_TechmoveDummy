using TechMove.Api.Models;

namespace TechMove.Api.Patterns.Observer
{
    // Notifies logistics managers when a contract status changes
    public class NotificationService : IContractObserver
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public void Update(ContractStatus newStatus, int contractId)
        {
            _logger.LogInformation("Contract {ContractId} changed to {Status} : logistics managers notified.", contractId, newStatus);
        }
    }
}
