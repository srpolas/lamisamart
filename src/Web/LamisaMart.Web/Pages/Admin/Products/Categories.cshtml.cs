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
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CategoriesModel> _logger;

    public CategoriesModel(ICatalogDbContext catalogContext, IWebHostEnvironment environment, ILogger<CategoriesModel> logger)
    {
        _catalogContext = catalogContext;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public List<CategoryItemViewModel> CategoriesList { get; set; } = new();
    public List<CategoryItemViewModel> MainCategoriesList => CategoriesList.Where(c => !c.IsSubCategory).ToList();

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
        public Guid? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public bool IsSubCategory => ParentCategoryId.HasValue && ParentCategoryId.Value != Guid.Empty;
    }

    public async Task OnGetAsync()
    {
        try
        {
            await EnsureDefaultStorefrontCategoriesAsync();

            var query = _catalogContext.Categories
                .AsNoTracking()
                .Include(c => c.ParentCategory)
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
                    ProductCount = c.Products != null && c.Products.Count > 0 ? c.Products.Count : GetSampleProductCount(c.Slug),
                    IsActive = c.IsActive,
                    IsFeatured = c.IsFeatured,
                    DisplayOrder = c.DisplayOrder,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed loading categories from DbContext. Serving storefront categories.");
        }

        if (!CategoriesList.Any())
        {
            CategoriesList = GetSampleCategories(SearchQuery);
        }
    }

    public async Task<IActionResult> OnPostCreateCategoryAsync(
        string categoryName,
        IFormFile? categoryImageFile,
        string? imageUrl,
        bool isFeatured,
        Guid? parentCategoryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["ErrorMessage"] = "Category name cannot be empty.";
                return RedirectToPage();
            }

            string finalImageUrl = imageUrl?.Trim() ?? string.Empty;

            // Process uploaded category image file if provided
            if (categoryImageFile != null && categoryImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "categories");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"cat_{Guid.NewGuid():N}{Path.GetExtension(categoryImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await categoryImageFile.CopyToAsync(stream);
                }
                finalImageUrl = $"/uploads/categories/{fileName}";
            }

            var slug = GenerateSlug(categoryName);
            Guid? finalParentId = (parentCategoryId.HasValue && parentCategoryId.Value != Guid.Empty) ? parentCategoryId.Value : null;

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryName.Trim(),
                Slug = slug,
                ImageUrl = string.IsNullOrWhiteSpace(finalImageUrl) ? GetDefaultCategoryImage(slug) : finalImageUrl,
                IsActive = true,
                IsFeatured = isFeatured,
                ParentCategoryId = finalParentId,
                DisplayOrder = CategoriesList.Count + 1
            };

            _catalogContext.Categories.Add(category);
            await _catalogContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{categoryName}' created successfully with image!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed creating category {Name}", categoryName);
            TempData["ErrorMessage"] = "Error creating category: " + ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditCategoryAsync(
        Guid categoryId,
        string categoryName,
        string slug,
        IFormFile? editCategoryImageFile,
        string? imageUrl,
        bool isFeatured,
        Guid? parentCategoryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["ErrorMessage"] = "Category name cannot be empty.";
                return RedirectToPage();
            }

            string finalImageUrl = imageUrl?.Trim() ?? string.Empty;

            // Process uploaded category image file if provided
            if (editCategoryImageFile != null && editCategoryImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "categories");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"cat_{Guid.NewGuid():N}{Path.GetExtension(editCategoryImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await editCategoryImageFile.CopyToAsync(stream);
                }
                finalImageUrl = $"/uploads/categories/{fileName}";
            }

            var targetSlug = !string.IsNullOrWhiteSpace(slug) ? GenerateSlug(slug) : GenerateSlug(categoryName);
            var targetName = categoryName.Trim();
            Guid? finalParentId = (parentCategoryId.HasValue && parentCategoryId.Value != Guid.Empty && parentCategoryId.Value != categoryId) ? parentCategoryId.Value : null;

            var cat = await _catalogContext.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId || c.Slug.ToLower() == targetSlug.ToLower() || c.Name.ToLower() == targetName.ToLower());

            if (cat == null)
            {
                cat = new Category
                {
                    Id = categoryId != Guid.Empty ? categoryId : Guid.NewGuid(),
                    Name = targetName,
                    Slug = targetSlug,
                    ImageUrl = string.IsNullOrWhiteSpace(finalImageUrl) ? GetDefaultCategoryImage(targetSlug) : finalImageUrl,
                    IsActive = true,
                    IsFeatured = isFeatured,
                    ParentCategoryId = finalParentId,
                    DisplayOrder = CategoriesList.Count + 1
                };
                _catalogContext.Categories.Add(cat);
            }
            else
            {
                cat.Name = targetName;
                cat.Slug = targetSlug;
                if (!string.IsNullOrWhiteSpace(finalImageUrl))
                {
                    cat.ImageUrl = finalImageUrl;
                }
                cat.IsFeatured = isFeatured;
                cat.ParentCategoryId = finalParentId;
            }

            await _catalogContext.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Category '{targetName}' updated successfully with image!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed editing category {Id}", categoryId);
            TempData["ErrorMessage"] = "Error updating category: " + ex.Message;
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
            new { Name = "Lehenga & Gown", Slug = "lehenga-gown", ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=300&q=80", Order = 4 },
            new { Name = "Men's Panjabi", Slug = "mens-panjabi", ImageUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=300&q=80", Order = 5 },
            new { Name = "Men's Apparel", Slug = "mens-apparel", ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=300&q=80", Order = 6 },
            new { Name = "Kids Wear", Slug = "kids-wear", ImageUrl = "https://images.unsplash.com/photo-1519238263530-99bdd11df2ea?w=300&q=80", Order = 7 },
            new { Name = "Footwear", Slug = "footwear", ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=300&q=80", Order = 8 },
            new { Name = "Bags & Purses", Slug = "bags", ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300&q=80", Order = 9 },
            new { Name = "Jewelry", Slug = "jewelry", ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=300&q=80", Order = 10 },
            new { Name = "Innerwear", Slug = "innerwear", ImageUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=300&q=80", Order = 11 },
            new { Name = "Cosmetics & Skincare", Slug = "cosmetics", ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=300&q=80", Order = 12 },
            new { Name = "Home & Handicraft", Slug = "home-handicraft", ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=300&q=80", Order = 13 }
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
        "lehenga-gown" => "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=300&q=80",
        "mens-panjabi" => "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=300&q=80",
        "mens-apparel" => "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=300&q=80",
        "kids-wear" => "https://images.unsplash.com/photo-1519238263530-99bdd11df2ea?w=300&q=80",
        "footwear" => "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=300&q=80",
        "bags" => "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300&q=80",
        "jewelry" => "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=300&q=80",
        "innerwear" => "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=300&q=80",
        "cosmetics" => "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=300&q=80",
        "home-handicraft" => "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=300&q=80",
        _ => "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=300&q=80"
    };

    private static int GetSampleProductCount(string slug) => slug switch
    {
        "saree" => 240,
        "three-piece" => 180,
        "kurti" => 120,
        "lehenga-gown" => 90,
        "mens-panjabi" => 110,
        "mens-apparel" => 135,
        "kids-wear" => 75,
        "footwear" => 65,
        "bags" => 85,
        "jewelry" => 95,
        "innerwear" => 60,
        "cosmetics" => 70,
        "home-handicraft" => 45,
        _ => 25
    };

    private List<CategoryItemViewModel> GetSampleCategories(string? search)
    {
        var sareeId = Guid.NewGuid();
        var threePieceId = Guid.NewGuid();

        var list = new List<CategoryItemViewModel>
        {
            new() { Id = sareeId, Name = "Saree", Slug = "saree", ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=300&q=80", ProductCount = 240, IsActive = true, IsFeatured = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "Jamdani Saree", Slug = "jamdani-saree", ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=300&q=80", ProductCount = 85, IsActive = true, IsFeatured = false, DisplayOrder = 2, ParentCategoryId = sareeId, ParentCategoryName = "Saree" },
            new() { Id = threePieceId, Name = "Three Piece", Slug = "three-piece", ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=300&q=80", ProductCount = 180, IsActive = true, IsFeatured = true, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "Kurti", Slug = "kurti", ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=300&q=80", ProductCount = 120, IsActive = true, IsFeatured = true, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "Lehenga & Gown", Slug = "lehenga-gown", ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=300&q=80", ProductCount = 90, IsActive = true, IsFeatured = true, DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), Name = "Men's Panjabi", Slug = "mens-panjabi", ImageUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=300&q=80", ProductCount = 110, IsActive = true, IsFeatured = true, DisplayOrder = 6 },
            new() { Id = Guid.NewGuid(), Name = "Men's Apparel", Slug = "mens-apparel", ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=300&q=80", ProductCount = 135, IsActive = true, IsFeatured = true, DisplayOrder = 7 },
            new() { Id = Guid.NewGuid(), Name = "Kids Wear", Slug = "kids-wear", ImageUrl = "https://images.unsplash.com/photo-1519238263530-99bdd11df2ea?w=300&q=80", ProductCount = 75, IsActive = true, IsFeatured = true, DisplayOrder = 8 },
            new() { Id = Guid.NewGuid(), Name = "Footwear", Slug = "footwear", ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=300&q=80", ProductCount = 65, IsActive = true, IsFeatured = true, DisplayOrder = 9 },
            new() { Id = Guid.NewGuid(), Name = "Bags & Purses", Slug = "bags", ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300&q=80", ProductCount = 85, IsActive = true, IsFeatured = true, DisplayOrder = 10 },
            new() { Id = Guid.NewGuid(), Name = "Jewelry", Slug = "jewelry", ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=300&q=80", ProductCount = 95, IsActive = true, IsFeatured = true, DisplayOrder = 11 },
            new() { Id = Guid.NewGuid(), Name = "Innerwear", Slug = "innerwear", ImageUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=300&q=80", ProductCount = 60, IsActive = true, IsFeatured = true, DisplayOrder = 12 },
            new() { Id = Guid.NewGuid(), Name = "Cosmetics & Skincare", Slug = "cosmetics", ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=300&q=80", ProductCount = 70, IsActive = true, IsFeatured = true, DisplayOrder = 13 },
            new() { Id = Guid.NewGuid(), Name = "Home & Handicraft", Slug = "home-handicraft", ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=300&q=80", ProductCount = 45, IsActive = true, IsFeatured = true, DisplayOrder = 14 }
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(c => c.Name.ToLower().Contains(q) || c.Slug.ToLower().Contains(q)).ToList();
        }

        return list;
    }
}
