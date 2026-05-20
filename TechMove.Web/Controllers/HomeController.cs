using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechMove.Web.Models;

namespace TechMove.Web.Controllers;

public class HomeController : Controller //A standard home controller that just controls pages (The IIE, 2026)
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy() //Privacy view is irrelevant
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
