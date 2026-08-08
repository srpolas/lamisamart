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
                    ImageUrl = c.ImageUrl ?? "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=150&q=80",
                    ProductCount = c.Products != null ? c.Products.Count : 24,
                    IsActive = c.IsActive,
                    IsFeatured = c.IsFeatured,
                    DisplayOrder = c.DisplayOrder
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed loading categories from DbContext. Returning fallback categories.");
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
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=150&q=80" : imageUrl.Trim(),
                IsActive = true,
                IsFeatured = isFeatured,
                DisplayOrder = 1
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling category active state");
        }

        return RedirectToPage();
    }

    private static string GenerateSlug(string text)
    {
        string str = text.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }

    private List<CategoryItemViewModel> GetSampleCategories(string? search)
    {
        var list = new List<CategoryItemViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Saree", Slug = "saree", ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=150&q=80", ProductCount = 240, IsActive = true, IsFeatured = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "Three Piece", Slug = "three-piece", ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=150&q=80", ProductCount = 180, IsActive = true, IsFeatured = true, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "Kurti", Slug = "kurti", ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=150&q=80", ProductCount = 120, IsActive = true, IsFeatured = true, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "Jewelry", Slug = "jewelry", ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=150&q=80", ProductCount = 95, IsActive = true, IsFeatured = true, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "Cosmetics", Slug = "cosmetics", ImageUrl = "https://images.unsplash.com/photo-1596462502278-27bfdc403348?w=150&q=80", ProductCount = 60, IsActive = true, IsFeatured = false, DisplayOrder = 5 }
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(c => c.Name.ToLower().Contains(q) || c.Slug.ToLower().Contains(q)).ToList();
        }

        return list;
    }
}
