using TechMove.Api.Models;

namespace TechMove.Api.Services
{
    // part 2 lost marks because not every status change was handled, so the rules for moving
    // a contract between statuses are written out here in one place and unit tested
    public static class ContractWorkflow
    {
        // which statuses you are allowed to move to from each status
        private static readonly Dictionary<ContractStatus, ContractStatus[]> Allowed = new()
        {
            [ContractStatus.Draft]   = new[] { ContractStatus.Active, ContractStatus.OnHold, ContractStatus.Expired },
            [ContractStatus.Active]  = new[] { ContractStatus.OnHold, ContractStatus.Expired },
            [ContractStatus.OnHold]  = new[] { ContractStatus.Active, ContractStatus.Expired },
            [ContractStatus.Expired] = new[] { ContractStatus.Active }
        };

        public static bool CanTransition(ContractStatus from, ContractStatus to)
            => Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

        // throws when the change is not allowed so the controller can turn it into a 400
        public static void EnsureCanTransition(ContractStatus from, ContractStatus to)
        {
            if (from == to)
                throw new InvalidOperationException($"The contract is already {from}.");

            if (!CanTransition(from, to))
                throw new InvalidOperationException($"A contract cannot move from {from} to {to}.");
        }
    }
}
