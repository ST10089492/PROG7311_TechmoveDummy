using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Patterns.Observer;
using TechMove.Api.Patterns.Strategy;
using TechMove.Api.Services;

// The IIE. 2026. LU 3: Enterprise software system development [PROG7311 Learn]. The Independent Institute of Education: Unpublished.

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Strategy Pattern = currency conversion (same wiring that used to live in the mvc project)
builder.Services.AddHttpClient<OpenExchangeStrategy>();
builder.Services.AddScoped<ICurrencyConversionStrategy, OpenExchangeStrategy>();
builder.Services.AddScoped<FinancialService>();

// Observer Pattern = contract status listeners
builder.Services.AddScoped<IContractObserver, NotificationService>();
builder.Services.AddScoped<IContractObserver, BillingService>();
builder.Services.AddScoped<IContractObserver, ComplianceService>();

// Application services
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<ServiceRequestService>();
builder.Services.AddScoped<FileValidationService>();

builder.Services.AddControllers();

// Swagger so the api documents itself and can be tested in the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // lets the uploaded agreement pdfs be downloaded
app.UseAuthorization();
app.MapControllers();

// run migrations on startup so the database is ready when the container comes up
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

// exposed so the integration test project can spin the api up in memory
public partial class Program { }
