using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Models;

namespace TechMove.Api.Services
{
    // keeps the client database work out of the controller, same as the other services do
    public class ClientService
    {
        private readonly AppDbContext _db;

        public ClientService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Client>> GetAllAsync()
            => await _db.Clients.Include(c => c.Contracts).ToListAsync();

        public async Task<Client?> GetByIdAsync(int id)
            => await _db.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<bool> ExistsAsync(int id)
            => await _db.Clients.AnyAsync(c => c.Id == id);

        public async Task<Client> CreateAsync(Client client)
        {
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();
            return client;
        }

        public async Task<bool> UpdateAsync(int id, Client updated)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null) return false;

            client.Name = updated.Name;
            client.ContactDetails = updated.ContactDetails;
            client.Region = updated.Region;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null) return false;

            _db.Clients.Remove(client);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
