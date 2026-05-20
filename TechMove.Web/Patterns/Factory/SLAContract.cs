namespace TechMove.Web.Patterns.Factory
{
    public class SLAContract : IContract
    {
        public string ServiceLevel { get; set; } = string.Empty;

        public string GetContractType() => "SLA";

        // SLA contracts MUST be Premium level
        public bool Validate()
        {
            return ServiceLevel == "Premium";
        }
    }

    public class SLAContractFactory : ContractFactory
    {
        public override IContract CreateContract() => new SLAContract();
    }
}
