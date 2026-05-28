using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechMove.Api.Data;
using TechMove.Api.Patterns.Strategy;

namespace TechMove.Tests
{
    // spins the real api up in memory but swaps the database and the currency call so the
    // integration tests do not need sql server or an internet connection (The IIE, 2026)
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // replace the sql server context with an in memory one
                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor != null) services.Remove(dbDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("techmove-integration-tests"));

                // use the fixed rate strategy so creating a service request never hits the live currency api
                var currencyDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICurrencyConversionStrategy));
                if (currencyDescriptor != null) services.Remove(currencyDescriptor);

                services.AddScoped<ICurrencyConversionStrategy, MockConversionStrategy>();
            });
        }
    }
}
