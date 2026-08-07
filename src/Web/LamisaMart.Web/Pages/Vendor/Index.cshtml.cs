using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Vendor;

// [Authorize(Roles = "Vendor")] // Commented out for UI demo
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
