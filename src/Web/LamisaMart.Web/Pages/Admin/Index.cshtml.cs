using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin;

// [Authorize(Roles = "SuperAdmin")] // Commented out for UI demo
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
