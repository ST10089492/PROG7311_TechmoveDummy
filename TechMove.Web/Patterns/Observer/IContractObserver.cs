using TechMove.Web.Models;

namespace TechMove.Web.Patterns.Observer
{
    // Observer interface (any service that needs to react to contract status changes implements this)
    public interface IContractObserver
    {
        void Update(ContractStatus newStatus, int contractId);
    }
}
