using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TechMove.Api.Data;
using TechMove.Api.Patterns.Observer;
using TechMove.Api.Patterns.Strategy;
using TechMove.Api.Services;

// The IIE. 2026. LU 3: Enterprise software system development [PROG7311 Learn]. The Independent Institute of Education: Unpublished.

var builder = WebApplication.CreateBuilder(args);

// Database
// connection string comes from config, but docker compose overrides it with an env var
// (ConnectionStrings__DefaultConnection) so the same build points at the sql server container.
// EnableRetryOnFailure lets the api keep trying while the database container is still starting up.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

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

// JWT authentication, the signing key and issuer come from config (env vars in the container)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();

// Swagger so the api documents itself and can be tested in the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // adds the Authorize button in swagger so a token can be pasted in for the protected calls
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT from /api/auth/login here."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // lets the uploaded agreement pdfs be downloaded
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// run migrations on startup so the database is ready when the container comes up
// the IsRelational check lets the in memory database used by the integration tests skip this
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
}

app.Run();

// exposed so the integration test project can spin the api up in memory
public partial class Program { }
