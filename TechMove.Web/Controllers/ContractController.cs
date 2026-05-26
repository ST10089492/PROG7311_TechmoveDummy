using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Web.ApiClients;
using TechMove.Web.Models;

namespace TechMove.Web.Controllers
{
    // the contract screens, dropdown data and the contract list all come from the api now
    public class ContractController : Controller
    {
        private readonly ContractApi _contractApi;
        private readonly ClientApi _clientApi;
        private readonly TokenStore _tokenStore;

        public ContractController(ContractApi contractApi, ClientApi clientApi, TokenStore tokenStore)
        {
            _contractApi = contractApi;
            _clientApi = clientApi;
            _tokenStore = tokenStore;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, ContractStatus? status)
        {
            try
            {
                var contracts = await _contractApi.GetAllAsync(from, to, status?.ToString());
                ViewBag.From = from?.ToString("yyyy-MM-dd");
                ViewBag.To = to?.ToString("yyyy-MM-dd");
                ViewBag.Status = status;
                return View(contracts);
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
                var contract = await _contractApi.GetByIdAsync(id);
                if (contract == null) return NotFound();
                return View(contract);
            }
            catch (HttpRequestException)
            {
                return View("ApiUnavailable");
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                await PopulateDropdowns();
                return View();
            }
            catch (HttpRequestException)
            {
                return View("ApiUnavailable");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile? signedAgreement)
        {
            if (!_tokenStore.IsLoggedIn) return RedirectToLogin();

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(contract);
                return View(contract);
            }

            var result = await _contractApi.CreateAsync(contract);
            if (!result.Ok)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                await PopulateDropdowns(contract);
                return View(contract);
            }

            // the contract was created, now send the pdf if one was picked
            if (signedAgreement != null && result.Value != null)
            {
                var upload = await _contractApi.UploadAgreementAsync(result.Value.Id, signedAgreement);
                if (!upload.Ok) TempData["Warning"] = upload.Error;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var contract = await _contractApi.GetByIdAsync(id);
                if (contract == null) return NotFound();
                await PopulateDropdowns(contract);
                return View(contract);
            }
            catch (HttpRequestException)
            {
                return View("ApiUnavailable");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? signedAgreement)
        {
            if (id != contract.Id) return BadRequest();
            if (!_tokenStore.IsLoggedIn) return RedirectToLogin();

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(contract);
                return View(contract);
            }

            var result = await _contractApi.UpdateAsync(id, contract);
            if (!result.Ok)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                await PopulateDropdowns(contract);
                return View(contract);
            }

            if (signedAgreement != null)
            {
                var upload = await _contractApi.UploadAgreementAsync(id, signedAgreement);
                if (!upload.Ok) TempData["Warning"] = upload.Error;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _contractApi.GetByIdAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_tokenStore.IsLoggedIn) return RedirectToLogin();
            await _contractApi.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // builds the client, service level and status dropdowns from the api
        private async Task PopulateDropdowns(Contract? contract = null)
        {
            var clients = await _clientApi.GetAllAsync();
            ViewBag.Clients = new SelectList(clients, "Id", "Name", contract?.ClientId);
            ViewBag.ServiceLevels = new SelectList(new[] { "Standard", "Premium", "International" }, contract?.ServiceLevel);
            ViewBag.Statuses = new SelectList(Enum.GetValues<ContractStatus>(), contract?.Status);
        }

        private IActionResult RedirectToLogin()
            => RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
    }
}
