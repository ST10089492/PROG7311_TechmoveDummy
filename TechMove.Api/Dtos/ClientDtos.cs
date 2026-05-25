using System.ComponentModel.DataAnnotations;

namespace TechMove.Api.Dtos
{
    public class ClientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactDetails { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        // the client screens show the contracts that belong to a client
        public List<ContractDto>? Contracts { get; set; }
    }

    public class CreateClientDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string ContactDetails { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Region { get; set; } = string.Empty;
    }
}
