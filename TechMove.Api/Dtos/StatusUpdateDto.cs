using System.ComponentModel.DataAnnotations;

namespace TechMove.Api.Dtos
{
    // used by the patch endpoints to approve, decline or move an item along
    public class StatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
