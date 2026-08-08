using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LamisaMart.Identity.Domain.Entities;

namespace LamisaMart.Identity.Infrastructure.Persistence;

public static class IdentitySeeder
{
    public const string SuperAdminRole = "SuperAdmin";
    public const string SuperUserRole = "SuperUser";
    public const string AdminRole = "Admin";
    public const string VendorRole = "Vendor";
    public const string CustomerRole = "Customer";

    public const string SuperUserEmail = "srpolas.bd@gmail.com";
    public const string AdminEmail = "admin@lamisamart.bd";
    public const string DefaultPassword = "@Admin123";

    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.EnsureCreatedAsync();

        // 1. Seed Roles
        string[] roles = [SuperAdminRole, SuperUserRole, AdminRole, VendorRole, CustomerRole];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role '{Role}' created.", role);
            }
        }

        // 2. Seed / Update Superusers
        var accountsToSeed = new[]
        {
            (Username: "srpolas.bd@gmail.com", Email: SuperUserEmail, Name: "Super User"),
            (Username: "admin", Email: AdminEmail, Name: "System Admin")
        };

        string[] superRoles = [SuperAdminRole, SuperUserRole, AdminRole];

        foreach (var acc in accountsToSeed)
        {
            var user = await userManager.FindByEmailAsync(acc.Email) ?? await userManager.FindByNameAsync(acc.Username);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = acc.Username,
                    Email = acc.Email,
                    FullName = acc.Name,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(user, DefaultPassword);
                if (createResult.Succeeded)
                {
                    logger.LogInformation("Super user '{Username}' ({Email}) created successfully.", acc.Username, acc.Email);
                }
                else
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create user {Username}: {Errors}", acc.Username, errors);
                }
            }
            else
            {
                // Reset password to default if needed
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, DefaultPassword);
            }

            if (user != null)
            {
                foreach (var role in superRoles)
                {
                    if (!await userManager.IsInRoleAsync(user, role))
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
            }
        }
    }
}
