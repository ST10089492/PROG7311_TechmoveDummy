using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.Api.Dtos;
using TechMove.Api.Models;
using TechMove.Api.Services;

namespace TechMove.Api.Controllers
{
    // the client crud goes through ClientService now, the controller just maps to and from dtos
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly ClientService _clientService;

        public ClientsController(ClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
        {
            var clients = await _clientService.GetAllAsync();
            return Ok(clients.Select(ToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetById(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return Ok(ToDto(client));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ClientDto>> Create(CreateClientDto dto)
        {
            var client = new Client
            {
                Name = dto.Name,
                ContactDetails = dto.ContactDetails,
                Region = dto.Region
            };

            await _clientService.CreateAsync(client);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, ToDto(client));
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateClientDto dto)
        {
            var updated = new Client
            {
                Name = dto.Name,
                ContactDetails = dto.ContactDetails,
                Region = dto.Region
            };

            if (!await _clientService.UpdateAsync(id, updated)) return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _clientService.DeleteAsync(id)) return NotFound();
            return NoContent();
        }

        private static ClientDto ToDto(Client c) => new ClientDto
        {
            Id = c.Id,
            Name = c.Name,
            ContactDetails = c.ContactDetails,
            Region = c.Region,
            Contracts = c.Contracts.Select(ct => new ContractDto
            {
                Id = ct.Id,
                Title = ct.Title,
                StartDate = ct.StartDate,
                EndDate = ct.EndDate,
                Status = ct.Status.ToString(),
                ServiceLevel = ct.ServiceLevel,
                SignedAgreementPath = ct.SignedAgreementPath,
                ClientId = ct.ClientId,
                ClientName = c.Name
            }).ToList()
        };
    }
}
