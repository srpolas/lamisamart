using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Users;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin,VendorAdmin")]
public class VendorTeamModel : PageModel
{
    private readonly ILogger<VendorTeamModel> _logger;

    public VendorTeamModel(ILogger<VendorTeamModel> logger)
    {
        _logger = logger;
    }

    public List<VendorStaffViewModel> StaffList { get; set; } = new();
    public List<string> VendorsList { get; set; } = new();

    public class VendorStaffViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Manager"; // Manager or SalesPerson
        public string VendorName { get; set; } = string.Empty;
        public bool CanManageProducts { get; set; } = true;
        public bool CanProcessOrders { get; set; } = true;
        public bool CanViewFinance { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime AddedAt { get; set; }
    }

    public void OnGet()
    {
        LoadVendors();
        StaffList = GetSampleStaff();
    }

    public IActionResult OnPostCreateStaffUser(
        string fullName,
        string email,
        string phone,
        string vendorName,
        string role,
        bool canManageProducts,
        bool canProcessOrders,
        bool canViewFinance)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Full name and email are required.";
            return RedirectToPage();
        }

        TempData["SuccessMessage"] = $"Team member '{fullName.Trim()}' added as '{role}' for store '{vendorName}'!";
        return RedirectToPage();
    }

    public IActionResult OnPostToggleStatus(Guid staffId)
    {
        TempData["SuccessMessage"] = "Staff status toggled successfully.";
        return RedirectToPage();
    }

    public IActionResult OnPostDeleteStaff(Guid staffId)
    {
        TempData["SuccessMessage"] = "Staff account deleted successfully.";
        return RedirectToPage();
    }

    private void LoadVendors()
    {
        VendorsList = new List<string>
        {
            "Narayanganj Weaver Guild",
            "Silk Emporium Rajshahi",
            "Nusrat Boutique",
            "Simple Elegance",
            "Crafts of Bengal",
            "Jamdani Artisan Collective",
            "Dhakai Heritage House"
        };
    }

    private List<VendorStaffViewModel> GetSampleStaff()
    {
        var now = DateTime.UtcNow;
        return new List<VendorStaffViewModel>
        {
            new() { Id = Guid.NewGuid(), FullName = "Kamrul Hasan", Email = "kamrul.mgr@narayanganjweavers.com", Phone = "01711223355", Role = "Manager", VendorName = "Narayanganj Weaver Guild", CanManageProducts = true, CanProcessOrders = true, CanViewFinance = true, IsActive = true, AddedAt = now.AddMonths(-4) },
            new() { Id = Guid.NewGuid(), FullName = "Fatema Begum", Email = "fatema.sales@narayanganjweavers.com", Phone = "01711223366", Role = "SalesPerson", VendorName = "Narayanganj Weaver Guild", CanManageProducts = false, CanProcessOrders = true, CanViewFinance = false, IsActive = true, AddedAt = now.AddMonths(-2) },
            new() { Id = Guid.NewGuid(), FullName = "Ariful Haque", Email = "arif.mgr@silkemporium.com", Phone = "01822334466", Role = "Manager", VendorName = "Silk Emporium Rajshahi", CanManageProducts = true, CanProcessOrders = true, CanViewFinance = false, IsActive = true, AddedAt = now.AddMonths(-3) },
            new() { Id = Guid.NewGuid(), FullName = "Sabrina Sultana", Email = "sabrina.sales@nusrat.com", Phone = "01933445577", Role = "SalesPerson", VendorName = "Nusrat Boutique", CanManageProducts = true, CanProcessOrders = true, CanViewFinance = false, IsActive = true, AddedAt = now.AddMonths(-1) }
        };
    }
}
