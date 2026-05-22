using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Dtos;
using TechMove.Api.Models;
using TechMove.Api.Services;

namespace TechMove.Api.Controllers
{
    // all the contract logic lives in ContractService which keeps the factory and observer
    // patterns on the backend, the controller just maps to and from dtos (The IIE, 2026)
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly ContractService _contractService;
        private readonly FileValidationService _fileService;
        private readonly AppDbContext _db;

        public ContractsController(ContractService contractService,
                                   FileValidationService fileService,
                                   AppDbContext db)
        {
            _contractService = contractService;
            _fileService = fileService;
            _db = db;
        }

        // GET /api/contracts with optional date range and status filtering
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetAll(DateTime? from, DateTime? to, string? status)
        {
            // keep the auto expire behaviour from part 2 so the list is up to date
            await _contractService.UpdateStatusesAsync();

            ContractStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<ContractStatus>(status, true, out var parsed))
                    return BadRequest($"'{status}' is not a valid contract status.");
                statusFilter = parsed;
            }

            var contracts = await _contractService.GetAllAsync(from, to, statusFilter);
            return Ok(contracts.Select(ToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContractDto>> GetById(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();
            return Ok(ToDto(contract));
        }

        [HttpPost]
        public async Task<ActionResult<ContractDto>> Create(CreateContractDto dto)
        {
            if (!await _db.Clients.AnyAsync(c => c.Id == dto.ClientId))
                return BadRequest($"Client {dto.ClientId} does not exist.");

            var contract = new Contract
            {
                Title = dto.Title,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ServiceLevel = dto.ServiceLevel,
                ClientId = dto.ClientId,
                Status = ContractStatus.Draft
            };

            try
            {
                // CreateAsync runs the factory validation and notifies the observers
                await _contractService.CreateAsync(contract);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            var saved = await _contractService.GetByIdAsync(contract.Id);
            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, ToDto(saved!));
        }

        // PATCH /api/contracts/5/status to approve, decline or put a contract on hold
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ContractDto>> UpdateStatus(int id, StatusUpdateDto dto)
        {
            if (!Enum.TryParse<ContractStatus>(dto.Status, true, out var newStatus))
                return BadRequest($"'{dto.Status}' is not a valid contract status.");

            var contract = await _contractService.ChangeStatusAsync(id, newStatus);
            if (contract == null) return NotFound();

            var updated = await _contractService.GetByIdAsync(id);
            return Ok(ToDto(updated!));
        }

        // uploads the signed agreement pdf for a contract (multipart form data)
        [HttpPost("{id}/agreement")]
        public async Task<IActionResult> UploadAgreement(int id, IFormFile file)
        {
            var contract = await _db.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            try
            {
                contract.SignedAgreementPath = await _fileService.SaveAsync(file);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("No file was provided.");
            }

            await _db.SaveChangesAsync();
            return Ok(new { contract.SignedAgreementPath });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();

            await _contractService.DeleteAsync(id);
            return NoContent();
        }

        private static ContractDto ToDto(Contract c) => new ContractDto
        {
            Id = c.Id,
            Title = c.Title,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Status = c.Status.ToString(),
            ServiceLevel = c.ServiceLevel,
            SignedAgreementPath = c.SignedAgreementPath,
            ClientId = c.ClientId,
            ClientName = c.Client?.Name
        };
    }
}
