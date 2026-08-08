using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class PageBuilderModel : PageModel
{
    public void OnGet()
    {
    }
}
