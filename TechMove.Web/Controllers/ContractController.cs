using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Web.Data;
using TechMove.Web.Models;
using TechMove.Web.Services;

namespace TechMove.Web.Controllers
{
    public class ContractController : Controller
    {
        private readonly ContractService _contractService; // Handles business logic for contracts (The IIE, 2026)
        private readonly FileValidationService _fileService; // Handles file validation
        private readonly AppDbContext _db; //dropdown data

        public ContractController(ContractService contractService,
                                  FileValidationService fileService,
                                  AppDbContext db)
        {
            _contractService = contractService;
            _fileService = fileService;
            _db = db;
        }

        // Displays list of contracts with Filter
        public async Task<IActionResult> Index(DateTime? from, DateTime? to, ContractStatus? status)
        {
            await _contractService.UpdateStatusesAsync();
            var contracts = await _contractService.GetAllAsync(from, to, status);
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            return View(contracts);
        }

        // Displays details of a specific contract
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        // Returns form for creating a new contract
        public IActionResult Create()
        {
            ViewBag.Clients = new SelectList(_db.Clients, "Id", "Name");
            ViewBag.ServiceLevels = new SelectList(new[] { "Standard", "Premium", "International" });
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(Contract contract, IFormFile? signedAgreement) // Handles contract creation and file upload
        {
            if (signedAgreement != null)
            {
                try
                {
                    contract.SignedAgreementPath = await _fileService.SaveAsync(signedAgreement);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("SignedAgreementPath", ex.Message);
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Clients = new SelectList(_db.Clients, "Id", "Name");
                ViewBag.ServiceLevels = new SelectList(new[] { "Standard", "Premium", "International" });
                return View(contract);
            }

            await _contractService.CreateAsync(contract);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)  // Returns form for editing a contract
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();
            ViewBag.Clients = new SelectList(_db.Clients, "Id", "Name", contract.ClientId);
            ViewBag.ServiceLevels = new SelectList(new[] { "Standard", "Premium", "International" }, contract.ServiceLevel);
            ViewBag.Statuses = new SelectList(Enum.GetValues<ContractStatus>(), contract.Status);
            return View(contract);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? signedAgreement) // Handles contract update and optional file replacement
        {
            if (id != contract.Id) return BadRequest();

            if (signedAgreement != null)
            {
                try
                {
                    contract.SignedAgreementPath = await _fileService.SaveAsync(signedAgreement);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("SignedAgreementPath", ex.Message);
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Clients = new SelectList(_db.Clients, "Id", "Name", contract.ClientId);
                ViewBag.ServiceLevels = new SelectList(new[] { "Standard", "Premium", "International" }, contract.ServiceLevel);
                ViewBag.Statuses = new SelectList(Enum.GetValues<ContractStatus>(), contract.Status);
                return View(contract);
            }

            await _contractService.UpdateAsync(contract);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)   // Returns confirmation view for deleting a contract
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken] // Confirmation deletion
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _contractService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
