using System.ComponentModel.DataAnnotations;

namespace TechMove.Api.Dtos
{
    // what we send back to the client for a contract
    // we flatten the client name so the json has no navigation loops (The IIE, 2026)
    public class ContractDto
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
    }

    // what the caller has to send to make a new contract
    public class CreateContractDto
    {
        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string ServiceLevel { get; set; } = string.Empty;

        [Required]
        public int ClientId { get; set; }
    }
}
