namespace TechMove.Api.Patterns.Factory
{
    public interface IContract
    {
        string GetContractType();
        bool Validate();
    }
}
