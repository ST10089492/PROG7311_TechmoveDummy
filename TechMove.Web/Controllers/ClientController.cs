using Microsoft.AspNetCore.Mvc;
using TechMove.Web.ApiClients;
using TechMove.Web.Models;

namespace TechMove.Web.Controllers
{
    // no more database here, every action goes through the api (The IIE, 2026)
    public class ClientController : Controller
    {
        private readonly ClientApi _clientApi;
        private readonly TokenStore _tokenStore;

        public ClientController(ClientApi clientApi, TokenStore tokenStore)
        {
            _clientApi = clientApi;
            _tokenStore = tokenStore;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _clientApi.GetAllAsync());
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
                var client = await _clientApi.GetByIdAsync(id);
                if (client == null) return NotFound();
                return View(client);
            }
            catch (HttpRequestException)
            {
                return View("ApiUnavailable");
            }
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!_tokenStore.IsLoggedIn) return RedirectToLogin();
            if (!ModelState.IsValid) return View(client);

            var result = await _clientApi.CreateAsync(client);
            if (!result.Ok)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(client);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientApi.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();
            if (!_tokenStore.IsLoggedIn) return RedirectToLogin();
            if (!ModelState.IsValid) return View(client);

            var result = await _clientApi.UpdateAsync(id, client);
            if (!result.Ok)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(client);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientApi.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_tokenStore.IsLoggedIn) return RedirectToLogin();
            await _clientApi.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private IActionResult RedirectToLogin()
            => RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
    }
}
