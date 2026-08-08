using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Products;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class AttributesModel : PageModel
{
    private readonly ILogger<AttributesModel> _logger;

    public AttributesModel(ILogger<AttributesModel> logger)
    {
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public List<AttributeViewModel> AttributesList { get; set; } = new();

    public class AttributeViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
        public int ProductCount { get; set; }
    }

    public void OnGet()
    {
        AttributesList = GetSampleAttributes(SearchQuery);
    }

    public IActionResult OnPostCreateAttribute(string name, string code, string values)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Attribute name is required.";
            return RedirectToPage();
        }

        TempData["SuccessMessage"] = $"Attribute '{name}' added successfully!";
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(Guid attributeId)
    {
        TempData["SuccessMessage"] = "Attribute deleted successfully.";
        return RedirectToPage();
    }

    private List<AttributeViewModel> GetSampleAttributes(string? search)
    {
        var list = new List<AttributeViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Color Variant", Code = "color", Values = new List<string> { "Ruby Red", "Royal Navy", "Emerald Green", "Pastel Peach", "Mustard Yellow" }, ProductCount = 320 },
            new() { Id = Guid.NewGuid(), Name = "Fabric Material", Code = "fabric", Values = new List<string> { "Pure Combed Cotton", "Mulberry Katan Silk", "Swiss Lawn", "Georgette" }, ProductCount = 280 },
            new() { Id = Guid.NewGuid(), Name = "Weave Count", Code = "weave_count", Values = new List<string> { "80 Count", "100 Count Pure", "120 Count Superfine" }, ProductCount = 195 },
            new() { Id = Guid.NewGuid(), Name = "Zari Type", Code = "zari_type", Values = new List<string> { "Gold Zari", "Silver Zari", "Antique Copper Zari" }, ProductCount = 140 },
            new() { Id = Guid.NewGuid(), Name = "Apparel Size", Code = "size", Values = new List<string> { "Small (36)", "Medium (38)", "Large (40)", "XL (42)", "XXL (44)" }, ProductCount = 110 }
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(a => a.Name.ToLower().Contains(q) || a.Code.ToLower().Contains(q)).ToList();
        }

        return list;
    }
}
