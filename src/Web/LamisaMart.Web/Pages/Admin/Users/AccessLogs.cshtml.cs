using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Users;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class AccessLogsModel : PageModel
{
    public List<AccessLogViewModel> LogsList { get; set; } = new();

    public class AccessLogViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string ControllerRoute { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public void OnGet()
    {
        var now = DateTime.UtcNow;
        LogsList = new List<AccessLogViewModel>
        {
            new() { Id = Guid.NewGuid(), UserName = "Kamrul Hasan (Manager)", Role = "Manager", ActionType = "Updated Product Stock", ControllerRoute = "/Admin/Products?handler=EditProduct", IpAddress = "103.205.71.14", Timestamp = now.AddMinutes(-12) },
            new() { Id = Guid.NewGuid(), UserName = "Fatema Begum (Sales)", Role = "SalesPerson", ActionType = "Printed Tax Invoice #LM-2026-8901", ControllerRoute = "/Admin/Business/Orders", IpAddress = "103.205.71.18", Timestamp = now.AddMinutes(-45) },
            new() { Id = Guid.NewGuid(), UserName = "Rafiqul Islam (VendorAdmin)", Role = "VendorAdmin", ActionType = "Authorized Staff Account", ControllerRoute = "/Admin/Users/VendorTeam", IpAddress = "103.205.71.14", Timestamp = now.AddHours(-3) },
            new() { Id = Guid.NewGuid(), UserName = "SuperAdmin", Role = "SuperAdmin", ActionType = "Updated Controller Permission Matrix", ControllerRoute = "/Admin/Users/Roles", IpAddress = "172.16.0.103", Timestamp = now.AddHours(-6) }
        };
    }
}
