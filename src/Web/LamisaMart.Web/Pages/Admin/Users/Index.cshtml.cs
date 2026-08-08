using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Users;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class IndexModel : PageModel
{
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

        UsersList = new List<UserAccountViewModel>
        {
            new() { Id = Guid.NewGuid(), FullName = "Super Admin User", Email = "admin@lamisamart.bd", Phone = "01700000000", Role = "SuperAdmin", AssociatedVendor = "Platform HQ", IsActive = true, CreatedAt = DateTime.UtcNow.AddYears(-1) },
            new() { Id = Guid.NewGuid(), FullName = "Rafiqul Islam", Email = "rafiq@narayanganjweavers.com", Phone = "01711223344", Role = "VendorAdmin", AssociatedVendor = "Narayanganj Weaver Guild", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-8) },
            new() { Id = Guid.NewGuid(), FullName = "Kamrul Hasan", Email = "kamrul.mgr@narayanganjweavers.com", Phone = "01711223355", Role = "Manager", AssociatedVendor = "Narayanganj Weaver Guild", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new() { Id = Guid.NewGuid(), FullName = "Fatema Begum", Email = "fatema.sales@narayanganjweavers.com", Phone = "01711223366", Role = "SalesPerson", AssociatedVendor = "Narayanganj Weaver Guild", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { Id = Guid.NewGuid(), FullName = "Tanvir Ahmed", Email = "tanvir@silkemporium.com", Phone = "01822334455", Role = "VendorAdmin", AssociatedVendor = "Silk Emporium Rajshahi", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-6) }
        };
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

        TempData["SuccessMessage"] = $"User '{fullName.Trim()}' updated successfully! Assigned Role: '{role}'.";
        return RedirectToPage();
    }

    public IActionResult OnPostToggleUserStatus(Guid userId)
    {
        TempData["SuccessMessage"] = "User status updated successfully!";
        return RedirectToPage();
    }
}
