using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.Web.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required, StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cost (USD)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUSD { get; set; }

        [Display(Name = "Cost (ZAR)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZAR { get; set; }

        public string Status { get; set; } = "Pending"; // Pending them InProgress and then Completed

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // FK  Contract
        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }
    }
}
