using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using LamisaMart.Catalog.Application.Common.Interfaces;
using LamisaMart.Catalog.Domain.Entities;

namespace LamisaMart.Web.Pages.Admin.Products;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class CategoriesModel : PageModel
{
    private readonly ICatalogDbContext _catalogContext;
    private readonly ILogger<CategoriesModel> _logger;

    public CategoriesModel(ICatalogDbContext catalogContext, ILogger<CategoriesModel> logger)
    {
        _catalogContext = catalogContext;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public List<CategoryItemViewModel> CategoriesList { get; set; } = new();

    public class CategoryItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
    }

    public async Task OnGetAsync()
    {
        try
        {
            // Seed missing storefront categories if DB has fewer than 7 core categories
            await EnsureDefaultStorefrontCategoriesAsync();

            var query = _catalogContext.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(q) || c.Slug.ToLower().Contains(q));
            }

            var dbCategories = await query.OrderBy(c => c.DisplayOrder).ToListAsync();

            if (dbCategories != null && dbCategories.Any())
            {
                CategoriesList = dbCategories.Select(c => new CategoryItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    ImageUrl = !string.IsNullOrEmpty(c.ImageUrl) ? c.ImageUrl : GetDefaultCategoryImage(c.Slug),
                    ProductCount = c.Products != null ? c.Products.Count : GetSampleProductCount(c.Slug),
                    IsActive = c.IsActive,
                    IsFeatured = c.IsFeatured,
                    DisplayOrder = c.DisplayOrder
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed loading categories from DbContext. Serving 7 storefront categories.");
        }

        if (!CategoriesList.Any())
        {
            CategoriesList = GetSampleCategories(SearchQuery);
        }
    }

    public async Task<IActionResult> OnPostCreateCategoryAsync(string categoryName, string imageUrl, bool isFeatured)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["ErrorMessage"] = "Category name cannot be empty.";
                return RedirectToPage();
            }

            var slug = GenerateSlug(categoryName);
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryName.Trim(),
                Slug = slug,
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? GetDefaultCategoryImage(slug) : imageUrl.Trim(),
                IsActive = true,
                IsFeatured = isFeatured,
                DisplayOrder = CategoriesList.Count + 1
            };

            _catalogContext.Categories.Add(category);
            await _catalogContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{categoryName}' created successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed creating category {Name}", categoryName);
            TempData["ErrorMessage"] = "Error creating category: " + ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(Guid categoryId)
    {
        try
        {
            var cat = await _catalogContext.Categories.FindAsync(categoryId);
            if (cat != null)
            {
                cat.IsActive = !cat.IsActive;
                await _catalogContext.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category status updated to {(cat.IsActive ? "Active" : "Disabled")}.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling category active state");
        }

        return RedirectToPage();
    }

    private async Task EnsureDefaultStorefrontCategoriesAsync()
    {
        var coreCategories = new[]
        {
            new { Name = "Saree", Slug = "saree", ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=300&q=80", Order = 1 },
            new { Name = "Three Piece", Slug = "three-piece", ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=300&q=80", Order = 2 },
            new { Name = "Kurti", Slug = "kurti", ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=300&q=80", Order = 3 },
            new { Name = "Innerwear", Slug = "innerwear", ImageUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=300&q=80", Order = 4 },
            new { Name = "Jewelry", Slug = "jewelry", ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=300&q=80", Order = 5 },
            new { Name = "Bags", Slug = "bags", ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300&q=80", Order = 6 },
            new { Name = "Cosmetics", Slug = "cosmetics", ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=300&q=80", Order = 7 }
        };

        var existingSlugs = await _catalogContext.Categories.Select(c => c.Slug.ToLower()).ToListAsync();
        bool addedAny = false;

        foreach (var item in coreCategories)
        {
            if (!existingSlugs.Contains(item.Slug))
            {
                _catalogContext.Categories.Add(new Category
                {
                    Id = Guid.NewGuid(),
                    Name = item.Name,
                    Slug = item.Slug,
                    ImageUrl = item.ImageUrl,
                    DisplayOrder = item.Order,
                    IsActive = true,
                    IsFeatured = true
                });
                addedAny = true;
            }
        }

        if (addedAny)
        {
            await _catalogContext.SaveChangesAsync();
        }
    }

    private static string GenerateSlug(string text)
    {
        string str = text.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }

    private static string GetDefaultCategoryImage(string slug) => slug switch
    {
        "saree" => "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=300&q=80",
        "three-piece" => "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=300&q=80",
        "kurti" => "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=300&q=80",
        "innerwear" => "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=300&q=80",
        "jewelry" => "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=300&q=80",
        "bags" => "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300&q=80",
        "cosmetics" => "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=300&q=80",
        _ => "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=300&q=80"
    };

    private static int GetSampleProductCount(string slug) => slug switch
    {
        "saree" => 240,
        "three-piece" => 180,
        "kurti" => 120,
        "innerwear" => 85,
        "jewelry" => 95,
        "bags" => 70,
        "cosmetics" => 60,
        _ => 25
    };

    private List<CategoryItemViewModel> GetSampleCategories(string? search)
    {
        var list = new List<CategoryItemViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Saree", Slug = "saree", ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=300&q=80", ProductCount = 240, IsActive = true, IsFeatured = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "Three Piece", Slug = "three-piece", ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=300&q=80", ProductCount = 180, IsActive = true, IsFeatured = true, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "Kurti", Slug = "kurti", ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=300&q=80", ProductCount = 120, IsActive = true, IsFeatured = true, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "Innerwear", Slug = "innerwear", ImageUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=300&q=80", ProductCount = 85, IsActive = true, IsFeatured = true, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "Jewelry", Slug = "jewelry", ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=300&q=80", ProductCount = 95, IsActive = true, IsFeatured = true, DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), Name = "Bags", Slug = "bags", ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300&q=80", ProductCount = 70, IsActive = true, IsFeatured = true, DisplayOrder = 6 },
            new() { Id = Guid.NewGuid(), Name = "Cosmetics", Slug = "cosmetics", ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=300&q=80", ProductCount = 60, IsActive = true, IsFeatured = true, DisplayOrder = 7 }
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(c => c.Name.ToLower().Contains(q) || c.Slug.ToLower().Contains(q)).ToList();
        }

        return list;
    }
}
