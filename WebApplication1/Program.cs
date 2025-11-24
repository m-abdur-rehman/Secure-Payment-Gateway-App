using AspNetCoreRateLimit;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Repositories;
using PaymentGateway.Repositories.Interfaces;
using PaymentGateway.Security;
using WebApplication1.Middleware;
using WebApplication1.Utilities;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddControllersWithViews(options =>
{
    options.MaxModelBindingCollectionSize = 100;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1048576;
    options.Limits.MaxRequestHeadersTotalSize = 32768;
    options.Limits.MaxRequestLineSize = 8192;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOptions();
builder.Services.AddMemoryCache();

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddDataProtection()
    .SetApplicationName("SecurePaymentGateway");

builder.Services.AddSingleton<IDataProtectionHelper, DataProtectionHelper>();
builder.Services.AddHttpClient<IForexService, ForexService>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-AntiForgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.SuppressXFrameOptionsHeader = false;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<SecureHeadersMiddleware>();
app.UseIpRateLimiting();
app.UseRouting();
app.UseMiddleware<SecureErrorHandlingMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Payment}/{action=Index}/{id?}");


app.Run();
