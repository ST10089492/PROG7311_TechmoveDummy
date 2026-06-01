using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ClientService _clientService;

        public ContractsController(ContractService contractService,
                                   FileValidationService fileService,
                                   ClientService clientService)
        {
            _contractService = contractService;
            _fileService = fileService;
            _clientService = clientService;
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

            // the detail screen also lists the service requests under the contract
            var dto = ToDto(contract);
            dto.ServiceRequests = contract.ServiceRequests.Select(ToServiceRequestDto).ToList();
            return Ok(dto);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ContractDto>> Create(CreateContractDto dto)
        {
            if (!await _clientService.ExistsAsync(dto.ClientId))
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

        // full update from the edit screen
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateContractDto dto)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();

            if (!Enum.TryParse<ContractStatus>(dto.Status, true, out var status))
                return BadRequest($"'{dto.Status}' is not a valid contract status.");

            if (!await _clientService.ExistsAsync(dto.ClientId))
                return BadRequest($"Client {dto.ClientId} does not exist.");

            contract.Title = dto.Title;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.ServiceLevel = dto.ServiceLevel;
            contract.Status = status;
            contract.ClientId = dto.ClientId;

            await _contractService.UpdateAsync(contract);
            return NoContent();
        }

        // PATCH /api/contracts/5/status to approve, decline or put a contract on hold
        [Authorize]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ContractDto>> UpdateStatus(int id, StatusUpdateDto dto)
        {
            if (!Enum.TryParse<ContractStatus>(dto.Status, true, out var newStatus))
                return BadRequest($"'{dto.Status}' is not a valid contract status.");

            try
            {
                var contract = await _contractService.ChangeStatusAsync(id, newStatus);
                if (contract == null) return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            var updated = await _contractService.GetByIdAsync(id);
            return Ok(ToDto(updated!));
        }

        // uploads the signed agreement pdf for a contract (multipart form data)
        [Authorize]
        [HttpPost("{id}/agreement")]
        public async Task<IActionResult> UploadAgreement(int id, IFormFile file)
        {
            string savedPath;
            try
            {
                savedPath = await _fileService.SaveAsync(file);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("No file was provided.");
            }

            if (!await _contractService.SetAgreementPathAsync(id, savedPath))
                return NotFound();

            return Ok(new { SignedAgreementPath = savedPath });
        }

        [Authorize]
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

        private static ServiceRequestDto ToServiceRequestDto(ServiceRequest s) => new ServiceRequestDto
        {
            Id = s.Id,
            Description = s.Description,
            CostUSD = s.CostUSD,
            CostZAR = s.CostZAR,
            Status = s.Status,
            CreatedOn = s.CreatedOn,
            ContractId = s.ContractId,
            ContractTitle = s.Contract?.Title
        };
    }
}
