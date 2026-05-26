using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Web.ApiClients;
using TechMove.Web.Models;

namespace TechMove.Web.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly ServiceRequestApi _srApi;
        private readonly ContractApi _contractApi;
        private readonly TokenStore _tokenStore;

        public ServiceRequestController(ServiceRequestApi srApi, ContractApi contractApi, TokenStore tokenStore)
        {
            _srApi = srApi;
            _contractApi = contractApi;
            _tokenStore = tokenStore;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _srApi.GetAllAsync());
            }
            catch (HttpRequestException)
            {
                return View("ApiUnavailable");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var sr = await _srApi.GetByIdAsync(id);
                if (sr == null) return NotFound();
                return View(sr);
            }
            catch (HttpRequestException)
            {
                return View("ApiUnavailable");
            }
        }

        public async Task<IActionResult> Create(int? contractId)
        {
            try
            {
                await PopulateContracts(contractId);
                return View(new ServiceRequest { ContractId = contractId ?? 0 });
            }
            catch (HttpRequestException)
            {
                return View("ApiUnavailable");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request)
        {
            if (!_tokenStore.IsLoggedIn) return RedirectToLogin();

            if (!ModelState.IsValid)
            {
                await PopulateContracts(request.ContractId);
                return View(request);
            }

            var result = await _srApi.CreateAsync(request);
            if (!result.Ok)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                await PopulateContracts(request.ContractId);
                return View(request);
            }

            // same warning as part 2 if the currency api could not give us a rate
            if (result.Value != null && result.Value.CostZAR == 0 && result.Value.CostUSD > 0)
                TempData["Warning"] = "Currency API was unavailable. ZAR cost could not be calculated and has been saved as R 0.00.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var sr = await _srApi.GetByIdAsync(id);
            if (sr == null) return NotFound();
            return View(sr);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_tokenStore.IsLoggedIn) return RedirectToLogin();
            await _srApi.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // the dropdown of contracts plus a small map of contract id to status for the client side warning
        private async Task PopulateContracts(int? contractId)
        {
            var contracts = await _contractApi.GetAllAsync(null, null, null);
            ViewBag.Contracts = new SelectList(contracts.Select(c => new { c.Id, c.Title }), "Id", "Title", contractId);
            ViewBag.ContractStatuses = contracts.ToDictionary(c => c.Id, c => c.Status.ToString());
        }

        private IActionResult RedirectToLogin()
            => RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
    }
}
