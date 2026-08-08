using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LamisaMart.Catalog.Infrastructure.Persistence;
using LamisaMart.Ordering.Infrastructure.Persistence;
using LamisaMart.Payments.Infrastructure.Persistence;
using LamisaMart.Vendors.Infrastructure.Persistence;
using LamisaMart.Accounting.Infrastructure.Persistence;
using LamisaMart.PageBuilder.Infrastructure.Persistence;
using LamisaMart.Identity.Infrastructure.Persistence;
using LamisaMart.Identity.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String & DbContext Registrations
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=172.16.0.103;Database=LamisaMart_db;TrustServerCertificate=True;MultipleActiveResultSets=true;";

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(connectionString, x => x.UseHierarchyId()));

builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<VendorsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<AccountingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<PageBuilderDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(connectionString));

// ASP.NET Core Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<IdentityDbContext>()
.AddDefaultTokenProviders();

// Bind Interfaces
builder.Services.AddScoped<LamisaMart.Catalog.Application.Common.Interfaces.ICatalogDbContext>(provider => provider.GetRequiredService<CatalogDbContext>());
builder.Services.AddScoped<LamisaMart.Ordering.Application.Common.Interfaces.IOrderingDbContext>(provider => provider.GetRequiredService<OrderingDbContext>());
builder.Services.AddScoped<LamisaMart.Payments.Application.Common.Interfaces.IPaymentsDbContext>(provider => provider.GetRequiredService<PaymentsDbContext>());
builder.Services.AddScoped<LamisaMart.Vendors.Application.Common.Interfaces.IVendorsDbContext>(provider => provider.GetRequiredService<VendorsDbContext>());
builder.Services.AddScoped<LamisaMart.Accounting.Application.Common.Interfaces.IAccountingDbContext>(provider => provider.GetRequiredService<AccountingDbContext>());
builder.Services.AddScoped<LamisaMart.PageBuilder.Application.Common.Interfaces.IPageBuilderDbContext>(provider => provider.GetRequiredService<PageBuilderDbContext>());

// 2. Add Services to Container
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(LamisaMart.Catalog.Application.DTOs.CategoryDto).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(LamisaMart.Ordering.Application.DTOs.OrderDto).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(LamisaMart.Payments.Application.Common.Interfaces.ISSLCommerzClient).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(LamisaMart.PageBuilder.Application.Layouts.Queries.GetPageLayoutQuery).Assembly);
});

// Payments Module Configuration
builder.Services.Configure<LamisaMart.Payments.Infrastructure.Settings.SSLCommerzSettings>(
    builder.Configuration.GetSection(LamisaMart.Payments.Infrastructure.Settings.SSLCommerzSettings.SectionName));

builder.Services.AddHttpClient<LamisaMart.Payments.Application.Common.Interfaces.ISSLCommerzClient, LamisaMart.Payments.Infrastructure.Services.SSLCommerzClient>();

var app = builder.Build();

// Seed Identity Database & Superuser
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await IdentitySeeder.SeedAsync(scope.ServiceProvider, logger);
}

// 3. Configure Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();

