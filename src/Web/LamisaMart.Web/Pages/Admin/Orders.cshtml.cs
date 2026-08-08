using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class OrdersModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Admin/Business/Orders");
    }
}
