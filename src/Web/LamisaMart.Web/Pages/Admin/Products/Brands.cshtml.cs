using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Products;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class BrandsModel : PageModel
{
    private readonly ILogger<BrandsModel> _logger;

    public BrandsModel(ILogger<BrandsModel> logger)
    {
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public List<BrandViewModel> BrandsList { get; set; } = new();

    public class BrandViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public void OnGet()
    {
        BrandsList = GetSampleBrands(SearchQuery);
    }

    public IActionResult OnPostCreateBrand(string brandName, string logoUrl, bool isFeatured)
    {
        if (string.IsNullOrWhiteSpace(brandName))
        {
            TempData["ErrorMessage"] = "Brand name is required.";
            return RedirectToPage();
        }

        TempData["SuccessMessage"] = $"Brand '{brandName}' added successfully!";
        return RedirectToPage();
    }

    public IActionResult OnPostToggleFeatured(Guid brandId)
    {
        TempData["SuccessMessage"] = "Toggled featured status for brand.";
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(Guid brandId)
    {
        TempData["SuccessMessage"] = "Brand deleted successfully.";
        return RedirectToPage();
    }

    private List<BrandViewModel> GetSampleBrands(string? search)
    {
        var list = new List<BrandViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Lamisa Heritage", Slug = "lamisa-heritage", LogoUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=150&q=80", ProductCount = 142, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
            new() { Id = Guid.NewGuid(), Name = "Nusrat Craft", Slug = "nusrat-craft", LogoUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=150&q=80", ProductCount = 88, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new() { Id = Guid.NewGuid(), Name = "Rajshahi Silk House", Slug = "rajshahi-silk-house", LogoUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=150&q=80", ProductCount = 64, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddMonths(-3) },
            new() { Id = Guid.NewGuid(), Name = "Tangail Weavers Co.", Slug = "tangail-weavers", LogoUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=150&q=80", ProductCount = 52, IsFeatured = false, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { Id = Guid.NewGuid(), Name = "Bengal Jewels", Slug = "bengal-jewels", LogoUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=150&q=80", ProductCount = 39, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) }
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(b => b.Name.ToLower().Contains(q) || b.Slug.ToLower().Contains(q)).ToList();
        }

        return list;
    }
}
