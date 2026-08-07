using Microsoft.EntityFrameworkCore;
using LamisaMart.Catalog.Infrastructure.Persistence;
using LamisaMart.Ordering.Infrastructure.Persistence;
using LamisaMart.Payments.Infrastructure.Persistence;
using LamisaMart.Vendors.Infrastructure.Persistence;
using LamisaMart.Accounting.Infrastructure.Persistence;
using LamisaMart.PageBuilder.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String & DbContext Registrations
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=172.16.0.103;Database=LamisaMart;TrustServerCertificate=True;MultipleActiveResultSets=true;";

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

// 2. Add Services to Container
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(LamisaMart.Catalog.Application.DTOs.CategoryDto).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(LamisaMart.Ordering.Application.DTOs.OrderDto).Assembly);
});

// Payments Module Configuration
builder.Services.Configure<LamisaMart.Payments.Infrastructure.Settings.SSLCommerzSettings>(
    builder.Configuration.GetSection(LamisaMart.Payments.Infrastructure.Settings.SSLCommerzSettings.SectionName));

builder.Services.AddHttpClient<LamisaMart.Payments.Application.Common.Interfaces.ISSLCommerzClient, LamisaMart.Payments.Infrastructure.Services.SSLCommerzClient>();

var app = builder.Build();

// 3. Configure Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
