namespace TechMove.Web.Patterns.Factory
{
    // Abstract factory (The IIE, 2026)
    public abstract class ContractFactory
    {
        public abstract IContract CreateContract();
    }
}
