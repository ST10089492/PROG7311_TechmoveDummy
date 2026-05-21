namespace TechMove.Api.Patterns.Observer
{
    // Subject interface for the Observer pattern
    public interface IContractSubject
    {
        void RegisterObserver(IContractObserver observer);
        void RemoveObserver(IContractObserver observer);
        void NotifyObservers(TechMove.Api.Models.ContractStatus newStatus, int contractId);
    }
}
