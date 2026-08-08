using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

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
    public List<VendorSelectViewModel> AvailableVendors { get; set; } = new();

    public class BrandViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> AuthorizedVendors { get; set; } = new();
    }

    public class VendorSelectViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public void OnGet()
    {
        LoadVendors();
        BrandsList = GetSampleBrands(SearchQuery);
    }

    public IActionResult OnPostCreateBrand(string brandName, string logoUrl, bool isFeatured, string[] selectedVendors)
    {
        if (string.IsNullOrWhiteSpace(brandName))
        {
            TempData["ErrorMessage"] = "Brand name is required.";
            return RedirectToPage();
        }

        var vendorCount = selectedVendors != null ? selectedVendors.Length : 0;
        TempData["SuccessMessage"] = $"Brand '{brandName.Trim()}' created successfully and authorized for {vendorCount} vendor(s)!";
        return RedirectToPage();
    }

    public IActionResult OnPostEditBrand(Guid brandId, string brandName, string slug, string logoUrl, bool isFeatured, string[] selectedVendors)
    {
        if (string.IsNullOrWhiteSpace(brandName))
        {
            TempData["ErrorMessage"] = "Brand name is required.";
            return RedirectToPage();
        }

        var vendorCount = selectedVendors != null ? selectedVendors.Length : 0;
        TempData["SuccessMessage"] = $"Brand '{brandName.Trim()}' updated successfully with {vendorCount} authorized vendor(s)!";
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

    private void LoadVendors()
    {
        AvailableVendors = new List<VendorSelectViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Narayanganj Weaver Guild" },
            new() { Id = Guid.NewGuid(), Name = "Silk Emporium Rajshahi" },
            new() { Id = Guid.NewGuid(), Name = "Nusrat Boutique" },
            new() { Id = Guid.NewGuid(), Name = "Simple Elegance" },
            new() { Id = Guid.NewGuid(), Name = "Crafts of Bengal" },
            new() { Id = Guid.NewGuid(), Name = "Jamdani Artisan Collective" },
            new() { Id = Guid.NewGuid(), Name = "Dhakai Heritage House" }
        };
    }

    private static string GenerateSlug(string text)
    {
        string str = text.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }

    private List<BrandViewModel> GetSampleBrands(string? search)
    {
        var list = new List<BrandViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Lamisa Heritage", Slug = "lamisa-heritage", LogoUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=150&q=80", ProductCount = 142, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddMonths(-6), AuthorizedVendors = new List<string> { "Narayanganj Weaver Guild", "Dhakai Heritage House" } },
            new() { Id = Guid.NewGuid(), Name = "Nusrat Craft", Slug = "nusrat-craft", LogoUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=150&q=80", ProductCount = 88, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddMonths(-4), AuthorizedVendors = new List<string> { "Nusrat Boutique", "Simple Elegance" } },
            new() { Id = Guid.NewGuid(), Name = "Rajshahi Silk House", Slug = "rajshahi-silk-house", LogoUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=150&q=80", ProductCount = 64, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddMonths(-3), AuthorizedVendors = new List<string> { "Silk Emporium Rajshahi" } },
            new() { Id = Guid.NewGuid(), Name = "Tangail Weavers Co.", Slug = "tangail-weavers", LogoUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=150&q=80", ProductCount = 52, IsFeatured = false, CreatedAt = DateTime.UtcNow.AddMonths(-2), AuthorizedVendors = new List<string> { "Jamdani Artisan Collective", "Narayanganj Weaver Guild" } },
            new() { Id = Guid.NewGuid(), Name = "Bengal Jewels", Slug = "bengal-jewels", LogoUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=150&q=80", ProductCount = 39, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddMonths(-1), AuthorizedVendors = new List<string> { "Crafts of Bengal" } }
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(b => b.Name.ToLower().Contains(q) || b.Slug.ToLower().Contains(q)).ToList();
        }

        return list;
    }
}
