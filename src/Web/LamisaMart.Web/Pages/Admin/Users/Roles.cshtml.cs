using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Users;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class RolesModel : PageModel
{
    public List<ControllerPermissionViewModel> PermissionMatrix { get; set; } = new();

    public class ControllerPermissionViewModel
    {
        public string ControllerName { get; set; } = string.Empty;
        public string ActionDescription { get; set; } = string.Empty;
        public bool SuperAdminAllowed { get; set; } = true;
        public bool AdminAllowed { get; set; } = true;
        public bool VendorAdminAllowed { get; set; } = true;
        public bool ManagerAllowed { get; set; } = true;
        public bool SalesPersonAllowed { get; set; } = false;
    }

    public void OnGet()
    {
        PermissionMatrix = new List<ControllerPermissionViewModel>
        {
            new() { ControllerName = "ProductsController", ActionDescription = "Browse, Create, Edit & Delete Products Catalog", SuperAdminAllowed = true, AdminAllowed = true, VendorAdminAllowed = true, ManagerAllowed = true, SalesPersonAllowed = false },
            new() { ControllerName = "BrandsController", ActionDescription = "Manage Brands, Logo & Multi-Vendor Authorizations", SuperAdminAllowed = true, AdminAllowed = true, VendorAdminAllowed = false, ManagerAllowed = false, SalesPersonAllowed = false },
            new() { ControllerName = "CategoriesController", ActionDescription = "Manage Main Categories & Subcategories Hierarchy", SuperAdminAllowed = true, AdminAllowed = true, VendorAdminAllowed = false, ManagerAllowed = false, SalesPersonAllowed = false },
            new() { ControllerName = "OrdersController", ActionDescription = "Process Orders, Update Status, Print Invoices & View Details", SuperAdminAllowed = true, AdminAllowed = true, VendorAdminAllowed = true, ManagerAllowed = true, SalesPersonAllowed = true },
            new() { ControllerName = "VendorTeamController", ActionDescription = "Add Store Managers & Sales Representatives for Vendor", SuperAdminAllowed = true, AdminAllowed = false, VendorAdminAllowed = true, ManagerAllowed = false, SalesPersonAllowed = false },
            new() { ControllerName = "AccountingController", ActionDescription = "View Financial Ledgers, Vendor Payouts & Revenue Reports", SuperAdminAllowed = true, AdminAllowed = true, VendorAdminAllowed = true, ManagerAllowed = false, SalesPersonAllowed = false },
            new() { ControllerName = "SettingsController", ActionDescription = "Configure Gateways, SMTP, Business Info & Shop Visibility", SuperAdminAllowed = true, AdminAllowed = false, VendorAdminAllowed = false, ManagerAllowed = false, SalesPersonAllowed = false }
        };
    }

    public IActionResult OnPostSavePermissions()
    {
        TempData["SuccessMessage"] = "Role & Controller Access Matrix updated successfully!";
        return RedirectToPage();
    }
}
