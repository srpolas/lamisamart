using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Users;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin,VendorAdmin")]
public class VendorTeamModel : PageModel
{
    private static readonly List<VendorStaffViewModel> MasterStaffStore = InitMasterStaffStore();
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
        StaffList = MasterStaffStore.ToList();
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

        var newStaff = new VendorStaffViewModel
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Email = email.Trim(),
            Phone = phone ?? string.Empty,
            VendorName = vendorName,
            Role = role,
            CanManageProducts = canManageProducts,
            CanProcessOrders = canProcessOrders,
            CanViewFinance = canViewFinance,
            IsActive = true,
            AddedAt = DateTime.UtcNow
        };

        MasterStaffStore.Add(newStaff);
        TempData["SuccessMessage"] = $"Team member '{fullName.Trim()}' added as '{role}' for store '{vendorName}'!";
        return RedirectToPage();
    }

    public IActionResult OnPostEditStaffUser(
        Guid staffId,
        string fullName,
        string email,
        string phone,
        string vendorName,
        string role,
        bool canManageProducts,
        bool canProcessOrders,
        bool canViewFinance,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Full name and email address are required.";
            return RedirectToPage();
        }

        var targetStaff = MasterStaffStore.FirstOrDefault(s => s.Id == staffId);
        if (targetStaff != null)
        {
            targetStaff.FullName = fullName.Trim();
            targetStaff.Email = email.Trim();
            targetStaff.Phone = phone ?? string.Empty;
            targetStaff.VendorName = vendorName;
            targetStaff.Role = role;
            targetStaff.CanManageProducts = canManageProducts;
            targetStaff.CanProcessOrders = canProcessOrders;
            targetStaff.CanViewFinance = canViewFinance;
            targetStaff.IsActive = isActive;
        }

        TempData["SuccessMessage"] = $"Team member '{fullName.Trim()}' updated successfully!";
        return RedirectToPage();
    }

    public IActionResult OnPostToggleStatus(Guid staffId)
    {
        var targetStaff = MasterStaffStore.FirstOrDefault(s => s.Id == staffId);
        if (targetStaff != null)
        {
            targetStaff.IsActive = !targetStaff.IsActive;
            var statusText = targetStaff.IsActive ? "Active (Unlocked)" : "Disabled (Locked)";
            TempData["SuccessMessage"] = $"Staff member '{targetStaff.FullName}' status changed to: {statusText}!";
        }
        return RedirectToPage();
    }

    public IActionResult OnPostDeleteStaff(Guid staffId)
    {
        var targetStaff = MasterStaffStore.FirstOrDefault(s => s.Id == staffId);
        if (targetStaff != null)
        {
            MasterStaffStore.Remove(targetStaff);
            TempData["SuccessMessage"] = $"Staff member '{targetStaff.FullName}' deleted successfully!";
        }
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

    private static List<VendorStaffViewModel> InitMasterStaffStore()
    {
        var now = DateTime.UtcNow;
        return new List<VendorStaffViewModel>
        {
            new() { Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"), FullName = "Kamrul Hasan", Email = "kamrul.mgr@narayanganjweavers.com", Phone = "01711223355", Role = "Manager", VendorName = "Narayanganj Weaver Guild", CanManageProducts = true, CanProcessOrders = true, CanViewFinance = true, IsActive = true, AddedAt = now.AddMonths(-4) },
            new() { Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"), FullName = "Fatema Begum", Email = "fatema.sales@narayanganjweavers.com", Phone = "01711223366", Role = "SalesPerson", VendorName = "Narayanganj Weaver Guild", CanManageProducts = false, CanProcessOrders = true, CanViewFinance = false, IsActive = true, AddedAt = now.AddMonths(-2) },
            new() { Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"), FullName = "Ariful Haque", Email = "arif.mgr@silkemporium.com", Phone = "01822334466", Role = "Manager", VendorName = "Silk Emporium Rajshahi", CanManageProducts = true, CanProcessOrders = true, CanViewFinance = false, IsActive = true, AddedAt = now.AddMonths(-3) },
            new() { Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"), FullName = "Sabrina Sultana", Email = "sabrina.sales@nusrat.com", Phone = "01933445577", Role = "SalesPerson", VendorName = "Nusrat Boutique", CanManageProducts = true, CanProcessOrders = true, CanViewFinance = false, IsActive = true, AddedAt = now.AddMonths(-1) }
        };
    }
}
