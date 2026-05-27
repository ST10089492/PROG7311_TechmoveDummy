namespace TechMove.Api.Services
{
    // the service request lifecycle was declared in part 2 but never actually enforced,
    // these rules make sure a request only moves Pending -> InProgress -> Completed (or Cancelled)
    public static class ServiceRequestWorkflow
    {
        public const string Pending = "Pending";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        private static readonly Dictionary<string, string[]> Allowed = new()
        {
            [Pending]    = new[] { InProgress, Cancelled },
            [InProgress] = new[] { Completed, Cancelled },
            [Completed]  = Array.Empty<string>(),
            [Cancelled]  = Array.Empty<string>()
        };

        public static bool IsKnownStatus(string status) => Allowed.ContainsKey(status);

        public static bool CanTransition(string from, string to)
            => Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

        public static void EnsureCanTransition(string from, string to)
        {
            if (!IsKnownStatus(to))
                throw new InvalidOperationException($"'{to}' is not a valid service request status.");

            if (from == to)
                throw new InvalidOperationException($"The service request is already {from}.");

            if (!CanTransition(from, to))
                throw new InvalidOperationException($"A service request cannot move from {from} to {to}.");
        }
    }
}
