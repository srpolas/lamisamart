using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Business;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class ReportsModel : PageModel
{
    public void OnGet()
    {
    }
}
