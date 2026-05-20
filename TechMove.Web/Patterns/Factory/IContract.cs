namespace TechMove.Web.Patterns.Factory
{
    public interface IContract
    {
        string GetContractType();
        bool Validate();
    }
}
