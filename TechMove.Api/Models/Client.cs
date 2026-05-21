using System.ComponentModel.DataAnnotations; // Provides validation attributes (The IIE, 2026)

namespace TechMove.Api.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(200)]
        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Region { get; set; } = string.Empty;

        // Navigation
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
