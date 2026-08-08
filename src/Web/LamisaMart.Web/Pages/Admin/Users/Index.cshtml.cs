using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Users;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class IndexModel : PageModel
{
    private static readonly List<UserAccountViewModel> MasterUserStore = InitMasterStore();

    public List<UserAccountViewModel> UsersList { get; set; } = new();
    public List<string> RolesList { get; set; } = new();
    public List<string> VendorsList { get; set; } = new();

    public class UserAccountViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AssociatedVendor { get; set; } = "Platform HQ";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public void OnGet()
    {
        RolesList = new List<string> { "SuperAdmin", "Admin", "VendorAdmin", "Manager", "SalesPerson", "User" };
        VendorsList = new List<string>
        {
            "Platform HQ",
            "Narayanganj Weaver Guild",
            "Silk Emporium Rajshahi",
            "Nusrat Boutique",
            "Simple Elegance",
            "Crafts of Bengal"
        };

        UsersList = MasterUserStore.ToList();
    }

    public IActionResult OnPostEditUser(
        Guid userId,
        string fullName,
        string email,
        string phone,
        string role,
        string associatedVendor,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Full name and email address are required.";
            return RedirectToPage();
        }

        var targetUser = MasterUserStore.FirstOrDefault(u => u.Id == userId);
        if (targetUser != null)
        {
            targetUser.FullName = fullName.Trim();
            targetUser.Email = email.Trim();
            targetUser.Phone = phone ?? string.Empty;
            targetUser.Role = role;
            targetUser.AssociatedVendor = associatedVendor ?? "Platform HQ";
            targetUser.IsActive = isActive;
        }

        TempData["SuccessMessage"] = $"User '{fullName.Trim()}' updated successfully! Role: '{role}'.";
        return RedirectToPage();
    }

    public IActionResult OnPostToggleUserStatus(Guid userId)
    {
        var targetUser = MasterUserStore.FirstOrDefault(u => u.Id == userId);
        if (targetUser != null)
        {
            targetUser.IsActive = !targetUser.IsActive;
            var statusText = targetUser.IsActive ? "Activated (Unlocked)" : "Suspended (Locked)";
            TempData["SuccessMessage"] = $"User '{targetUser.FullName}' account status changed to: {statusText}!";
        }
        return RedirectToPage();
    }

    public IActionResult OnPostDeleteUser(Guid userId)
    {
        var targetUser = MasterUserStore.FirstOrDefault(u => u.Id == userId);
        if (targetUser != null)
        {
            MasterUserStore.Remove(targetUser);
            TempData["SuccessMessage"] = $"User account '{targetUser.FullName}' deleted successfully!";
        }
        return RedirectToPage();
    }

    private static List<UserAccountViewModel> InitMasterStore()
    {
        var now = DateTime.UtcNow;
        return new List<UserAccountViewModel>
        {
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), FullName = "Super Admin User", Email = "admin@lamisamart.bd", Phone = "01700000000", Role = "SuperAdmin", AssociatedVendor = "Platform HQ", IsActive = true, CreatedAt = now.AddYears(-1) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), FullName = "Rafiqul Islam", Email = "rafiq@narayanganjweavers.com", Phone = "01711223344", Role = "VendorAdmin", AssociatedVendor = "Narayanganj Weaver Guild", IsActive = true, CreatedAt = now.AddMonths(-8) },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), FullName = "Kamrul Hasan", Email = "kamrul.mgr@narayanganjweavers.com", Phone = "01711223355", Role = "Manager", AssociatedVendor = "Narayanganj Weaver Guild", IsActive = true, CreatedAt = now.AddMonths(-4) },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), FullName = "Fatema Begum", Email = "fatema.sales@narayanganjweavers.com", Phone = "01711223366", Role = "SalesPerson", AssociatedVendor = "Narayanganj Weaver Guild", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), FullName = "Tanvir Ahmed", Email = "tanvir@silkemporium.com", Phone = "01822334455", Role = "VendorAdmin", AssociatedVendor = "Silk Emporium Rajshahi", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-6) }
        };
    }
}
