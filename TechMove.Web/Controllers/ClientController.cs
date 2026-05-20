using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Web.Data;
using TechMove.Web.Models;

namespace TechMove.Web.Controllers
{
    public class ClientController : Controller //(The IIE, 2026)
    {
        private readonly AppDbContext _db; //CRUD operations

        public ClientController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index() // // Displays list of clients with their related contracts
            => View(await _db.Clients.Include(c => c.Contracts).ToListAsync());

        public async Task<IActionResult> Details(int id) // Displays details for a single client
        {
            var client = await _db.Clients.Include(c => c.Contracts)
                                          .FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        public IActionResult Create() => View();  // Returns view for creating a new client

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)  // Returns view for editing a new client
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();
            if (!ModelState.IsValid) return View(client);
            _db.Clients.Update(client);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)  // Returns view for deleting a new client
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client != null) { _db.Clients.Remove(client); await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
