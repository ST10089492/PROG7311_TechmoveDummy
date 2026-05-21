namespace TechMove.Api.Patterns.Factory
{
    public class FreightContract : IContract //(The IIE, 2026)
    {
        public string ServiceLevel { get; set; } = string.Empty;

        public string GetContractType() => "Freight";

        // Freight contracts only valid at Standard level (The IIE, 2026)
        public bool Validate()
        {
            return ServiceLevel == "Standard";
        }
    }

    public class FreightContractFactory : ContractFactory
    {
        public override IContract CreateContract() => new FreightContract();
    }
}
