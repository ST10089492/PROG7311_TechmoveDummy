using Microsoft.AspNetCore.Mvc;
using TechMove.Web.ApiClients;

namespace TechMove.Web.Controllers
{
    // handles the login form, on success it stores the jwt that the api handed back (The IIE, 2026)
    public class AccountController : Controller
    {
        private readonly AuthApi _authApi;
        private readonly TokenStore _tokenStore;

        public AccountController(AuthApi authApi, TokenStore tokenStore)
        {
            _authApi = authApi;
            _tokenStore = tokenStore;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var token = await _authApi.LoginAsync(username, password);
            if (token == null)
            {
                ModelState.AddModelError(string.Empty, "Wrong username or password, or the API is unavailable.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            _tokenStore.Save(token);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            _tokenStore.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
