using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.Api.Dtos;
using TechMove.Api.Models;
using TechMove.Api.Services;

namespace TechMove.Api.Controllers
{
    // the workflow rule (no requests on expired or on hold contracts) and the usd to zar
    // conversion both happen inside ServiceRequestService (The IIE, 2026)
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ServiceRequestService _srService;

        public ServiceRequestsController(ServiceRequestService srService)
        {
            _srService = srService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetAll()
        {
            var requests = await _srService.GetAllAsync();
            return Ok(requests.Select(ToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestDto>> GetById(int id)
        {
            var sr = await _srService.GetByIdAsync(id);
            if (sr == null) return NotFound();
            return Ok(ToDto(sr));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ServiceRequestDto>> Create(CreateServiceRequestDto dto)
        {
            var request = new ServiceRequest
            {
                Description = dto.Description,
                CostUSD = dto.CostUSD,
                ContractId = dto.ContractId
            };

            try
            {
                // this checks the contract status and works out the zar cost from the api
                await _srService.CreateAsync(request);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            var saved = await _srService.GetByIdAsync(request.Id);
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, ToDto(saved!));
        }

        // PATCH /api/servicerequests/5/status to move it to InProgress or Completed
        [Authorize]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ServiceRequestDto>> UpdateStatus(int id, StatusUpdateDto dto)
        {
            try
            {
                var sr = await _srService.ChangeStatusAsync(id, dto.Status);
                if (sr == null) return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            var updated = await _srService.GetByIdAsync(id);
            return Ok(ToDto(updated!));
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sr = await _srService.GetByIdAsync(id);
            if (sr == null) return NotFound();

            await _srService.DeleteAsync(id);
            return NoContent();
        }

        private static ServiceRequestDto ToDto(ServiceRequest s) => new ServiceRequestDto
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
