using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Web.Data;
using TechMove.Web.Models;
using TechMove.Web.Services;

namespace TechMove.Web.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly ServiceRequestService _srService;
        private readonly AppDbContext _db;

        public ServiceRequestController(ServiceRequestService srService, AppDbContext db)
        {
            _srService = srService;
            _db = db;
        }

        public async Task<IActionResult> Index()  // Displays service requests
            => View(await _srService.GetAllAsync());

        public async Task<IActionResult> Details(int id) // Displays details of a specific service request
        {
            var sr = await _srService.GetByIdAsync(id);
            if (sr == null) return NotFound();
            return View(sr);
        }

        public IActionResult Create(int? contractId) // Returns form for creating a new service request
        {
            ViewBag.Contracts = new SelectList(_db.Contracts.Select(c => new { c.Id, c.Title }), "Id", "Title", contractId);
            ViewBag.ContractStatuses = _db.Contracts.ToDictionary(c => c.Id, c => c.Status.ToString());
            return View(new ServiceRequest { ContractId = contractId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request) // Handles creation of a service request
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Contracts = new SelectList(_db.Contracts.Select(c => new { c.Id, c.Title }), "Id", "Title", request.ContractId);
                ViewBag.ContractStatuses = _db.Contracts.ToDictionary(c => c.Id, c => c.Status.ToString());
                return View(request);
            }

            try
            {
                await _srService.CreateAsync(request);

                if (request.CostZAR == 0 && request.CostUSD > 0)
                    TempData["Warning"] = "Currency API was unavailable. ZAR cost could not be calculated and has been saved as R 0.00. You can edit this request once the API is restored.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Contracts = new SelectList(_db.Contracts.Select(c => new { c.Id, c.Title }), "Id", "Title", request.ContractId);
                ViewBag.ContractStatuses = _db.Contracts.ToDictionary(c => c.Id, c => c.Status.ToString());
                return View(request);
            }
        }

        public async Task<IActionResult> Delete(int id)  // Returns confirmation view for deleting
        {
            var sr = await _srService.GetByIdAsync(id);
            if (sr == null) return NotFound();
            return View(sr);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // Handles confirmed deletion
        {
            await _srService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
