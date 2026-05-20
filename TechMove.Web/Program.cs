using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TechMove.Web.Data;
using TechMove.Web.Patterns.Observer; // Observer pattern implementation (The IIE, 2026)
using TechMove.Web.Patterns.Strategy; // Strategy pattern implementation (The IIE, 2026)
using TechMove.Web.Services; // Business logic services layer


//HTML & CSS Full Course. 2025. YouTube video, added by Code, B. [Online]. Available at: https://www.youtube.com/watch?v=HGTJBPNC-Gw [Accessed 10 April 2025].HTML & CSS Full Course. 2025. YouTube video, added by Code, B. [Online]. Available at: https://www.youtube.com/watch?v=HGTJBPNC-Gw [Accessed 10 April 2025].
//The IIE. 2026. LU 3: Enterprise software system development [PROG7311 Learn]. The Independent Institute of Education: Unpublished.
//The IIE. 2026. LU 4: Optimising application performance [PROG7311 Learn]. The Independent Institute of Education: Unpublished.The IIE. 2026. LU 4: Optimising application performance [PROG7311 Learn]. The Independent Institute of Education: Unpublished.

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Strategy Pattern = currency conversion
// Registers HTTP client for external API calls the currency exchnage rate
builder.Services.AddHttpClient<OpenExchangeStrategy>();
builder.Services.AddScoped<ICurrencyConversionStrategy, OpenExchangeStrategy>();
builder.Services.AddScoped<FinancialService>();

// Observer Pattern = contract status listeners
// Registers multiple observers that listen for contract changes
builder.Services.AddScoped<IContractObserver, NotificationService>();
builder.Services.AddScoped<IContractObserver, BillingService>();
builder.Services.AddScoped<IContractObserver, ComplianceService>();

// Application services
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<ServiceRequestService>();
builder.Services.AddScoped<FileValidationService>();


// Adds MVC controllers and views () 
// Frontend structure and styling concepts based on HTML & CSS fundamentals (Code, 2025)
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Runs migrations automatically in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run(); // Starts the web application
