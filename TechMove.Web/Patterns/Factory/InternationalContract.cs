namespace TechMove.Web.Patterns.Factory
{
    public class InternationalContract : IContract
    {
        public string ServiceLevel { get; set; } = string.Empty;

        public string GetContractType() => "International";

        // International contracts MUST use the International tier
        public bool Validate()
        {
            return ServiceLevel == "International";
        }
    }

    public class InternationalContractFactory : ContractFactory
    {
        public override IContract CreateContract() => new InternationalContract();
    }
}
