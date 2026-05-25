using TechMove.Web.Models;

namespace TechMove.Web.ApiClients
{
    // these mirror the json the api sends back, the ToModel methods turn them into the
    // view models the existing razor views already bind to (The IIE, 2026)

    public class ContractResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ServiceLevel { get; set; } = string.Empty;
        public string? SignedAgreementPath { get; set; }
        public int ClientId { get; set; }
        public string? ClientName { get; set; }
        public List<ServiceRequestResponse>? ServiceRequests { get; set; }

        public Contract ToModel() => new Contract
        {
            Id = Id,
            Title = Title,
            StartDate = StartDate,
            EndDate = EndDate,
            Status = Enum.TryParse<ContractStatus>(Status, out var s) ? s : ContractStatus.Draft,
            ServiceLevel = ServiceLevel,
            SignedAgreementPath = SignedAgreementPath,
            ClientId = ClientId,
            Client = ClientName == null ? null : new Client { Id = ClientId, Name = ClientName },
            ServiceRequests = ServiceRequests?.Select(r => r.ToModel()).ToList() ?? new List<ServiceRequest>()
        };
    }

    public class ClientResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactDetails { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public List<ContractResponse>? Contracts { get; set; }

        public Client ToModel() => new Client
        {
            Id = Id,
            Name = Name,
            ContactDetails = ContactDetails,
            Region = Region,
            Contracts = Contracts?.Select(c => c.ToModel()).ToList() ?? new List<Contract>()
        };
    }

    public class ServiceRequestResponse
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public int ContractId { get; set; }
        public string? ContractTitle { get; set; }

        public ServiceRequest ToModel() => new ServiceRequest
        {
            Id = Id,
            Description = Description,
            CostUSD = CostUSD,
            CostZAR = CostZAR,
            Status = Status,
            CreatedOn = CreatedOn,
            ContractId = ContractId,
            Contract = ContractTitle == null ? null : new Contract { Id = ContractId, Title = ContractTitle }
        };
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
