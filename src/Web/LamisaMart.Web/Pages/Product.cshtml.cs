using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages;

public class ProductModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Slug { get; set; } = string.Empty;

    public void OnGet()
    {
        // Fetch product from catalog module by slug
        // Using stub data for UI
    }
}
