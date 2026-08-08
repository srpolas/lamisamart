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
    public const string SuperUserPassword = "@Admin123";

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

        // 2. Seed / Update Superuser
        var superUser = await userManager.FindByEmailAsync(SuperUserEmail);
        if (superUser == null)
        {
            superUser = new ApplicationUser
            {
                UserName = SuperUserEmail,
                Email = SuperUserEmail,
                FullName = "Super User",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(superUser, SuperUserPassword);
            if (createResult.Succeeded)
            {
                logger.LogInformation("Super user '{Email}' created successfully.", SuperUserEmail);
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create super user: {Errors}", errors);
                throw new Exception($"Failed to create super user: {errors}");
            }
        }
        else
        {
            // Ensure password is set to @Admin123 if changed
            var token = await userManager.GeneratePasswordResetTokenAsync(superUser);
            var resetResult = await userManager.ResetPasswordAsync(superUser, token, SuperUserPassword);
            if (resetResult.Succeeded)
            {
                logger.LogInformation("Password for super user '{Email}' updated successfully.", SuperUserEmail);
            }
        }

        // 3. Ensure Superuser roles
        string[] userRoles = [SuperAdminRole, SuperUserRole, AdminRole];
        foreach (var role in userRoles)
        {
            if (!await userManager.IsInRoleAsync(superUser, role))
            {
                await userManager.AddToRoleAsync(superUser, role);
                logger.LogInformation("Assigned role '{Role}' to super user '{Email}'.", role, SuperUserEmail);
            }
        }
    }
}
