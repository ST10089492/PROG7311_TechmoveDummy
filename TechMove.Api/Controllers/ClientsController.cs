using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Dtos;
using TechMove.Api.Models;

namespace TechMove.Api.Controllers
{
    // clients are plain crud so the controller can talk to the context directly
    // there is no business rule on a client (The IIE, 2026)
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ClientsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
        {
            var clients = await _db.Clients.ToListAsync();
            return Ok(clients.Select(ToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetById(int id)
        {
            var client = await _db.Clients.FindAsync(id);
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

            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = client.Id }, ToDto(client));
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateClientDto dto)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null) return NotFound();

            client.Name = dto.Name;
            client.ContactDetails = dto.ContactDetails;
            client.Region = dto.Region;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null) return NotFound();

            _db.Clients.Remove(client);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static ClientDto ToDto(Client c) => new ClientDto
        {
            Id = c.Id,
            Name = c.Name,
            ContactDetails = c.ContactDetails,
            Region = c.Region
        };
    }
}
