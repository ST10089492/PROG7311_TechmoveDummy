using System.ComponentModel.DataAnnotations;

namespace TechMove.Api.Dtos
{
    public class ServiceRequestDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public int ContractId { get; set; }
        public string? ContractTitle { get; set; }
    }

    // the zar cost is worked out by the api using the currency strategy, the caller only gives usd
    public class CreateServiceRequestDto
    {
        [Required, StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal CostUSD { get; set; }

        [Required]
        public int ContractId { get; set; }
    }
}
