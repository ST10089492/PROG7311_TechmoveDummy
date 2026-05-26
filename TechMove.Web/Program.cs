using TechMove.Web.ApiClients;

// The IIE. 2026. LU 3: Enterprise software system development [PROG7311 Learn]. The Independent Institute of Education: Unpublished.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// session is where the jwt lives after the user logs in
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<TokenStore>();
builder.Services.AddTransient<AuthTokenHandler>();

// the api base url comes from config, docker compose overrides it with the api service name
var apiBaseUrl = (builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7257").TrimEnd('/') + "/";

// typed http clients, all the api calls go through these instead of a database
builder.Services.AddHttpClient<AuthApi>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ClientApi>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<ContractApi>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<ServiceRequestApi>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthTokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
