using Microsoft.EntityFrameworkCore;
using TechMove.Api.Models;

namespace TechMove.Api.Data
{
    public class AppDbContext : DbContext //(Code, 2025)
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client to Contracts one-to-many   (The IIE, 2026)
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contract to ServiceRequests one-to-many  (The IIE, 2026)
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal columns (Code, 2025)
            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.CostUSD)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.CostZAR)
                .HasColumnType("decimal(18,2)");

            // Seed data (Code, 2025)
            modelBuilder.Entity<Client>().HasData(
                new Client { Id = 1, Name = "Acme Freight Co.", ContactDetails = "acme@freight.com | +27 11 000 0001", Region = "Africa" },
                new Client { Id = 2, Name = "GlobalTech Logistics", ContactDetails = "ops@globaltech.com | +1 212 000 0002", Region = "North America" }
            );

            modelBuilder.Entity<Contract>().HasData(
                new Contract
                {
                    Id = 1, Title = "Standard Freight Agreement",
                    StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31),
                    Status = ContractStatus.Active, ServiceLevel = "Standard", ClientId = 1
                },
                new Contract
                {
                    Id = 2, Title = "Premium SLA Contract",
                    StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2026, 1, 1),
                    Status = ContractStatus.Expired, ServiceLevel = "Premium", ClientId = 2
                }
            );
        }
    }
}
