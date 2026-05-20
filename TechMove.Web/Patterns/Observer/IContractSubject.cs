namespace TechMove.Web.Patterns.Observer
{
    // Subject interface for the Observer pattern
    public interface IContractSubject
    {
        void RegisterObserver(IContractObserver observer);
        void RemoveObserver(IContractObserver observer);
        void NotifyObservers(TechMove.Web.Models.ContractStatus newStatus, int contractId);
    }
}
