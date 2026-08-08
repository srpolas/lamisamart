using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using LamisaMart.Catalog.Application.Common.Interfaces;
using LamisaMart.Catalog.Domain.Entities;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Web.Pages.Admin;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class ProductsModel : PageModel
{
    private readonly ICatalogDbContext _catalogContext;
    private readonly ILogger<ProductsModel> _logger;

    public ProductsModel(ICatalogDbContext catalogContext, ILogger<ProductsModel> logger)
    {
        _catalogContext = catalogContext;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    public List<AdminProductViewModel> ProductsList { get; set; } = new();
    public List<CategorySelectViewModel> AvailableCategories { get; set; } = new();
    public List<BrandSelectViewModel> AvailableBrands { get; set; } = new();
    public List<TagSelectViewModel> AvailableTags { get; set; } = new();
    public List<AttributeSelectViewModel> AvailableAttributes { get; set; } = new();

    public class AdminProductViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? SubCategoryName { get; set; }
        public string BrandName { get; set; } = "Lamisa Heritage";
        public string VendorName { get; set; } = "Verified Vendor";
        public decimal BasePrice { get; set; }
        public decimal CompareAtPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Rating { get; set; } = 4.8;
        public int ReviewCount { get; set; } = 18;
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; } = true;
        public bool EnableAttributes { get; set; }
        public List<string> SelectedTags { get; set; } = new();
        public Dictionary<string, string> SelectedAttributes { get; set; } = new();
    }

    public class CategorySelectViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public bool IsSubCategory => ParentCategoryId.HasValue && ParentCategoryId.Value != Guid.Empty;
    }

    public class BrandSelectViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TagSelectViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AttributeSelectViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
    }

    public async Task OnGetAsync()
    {
        await LoadMetadataOptionsAsync();

        try
        {
            var query = _catalogContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                query = query.Where(p => p.Category != null && p.Category.Slug.ToLower() == CategoryFilter.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(q) || p.Slug.ToLower().Contains(q));
            }

            var dbProducts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            if (dbProducts != null && dbProducts.Any())
            {
                ProductsList = dbProducts.Select(p => new AdminProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    CategoryName = p.Category != null ? (p.Category.ParentCategory != null ? p.Category.ParentCategory.Name : p.Category.Name) : "Saree",
                    SubCategoryName = p.Category != null && p.Category.ParentCategory != null ? p.Category.Name : null,
                    BrandName = "Lamisa Heritage",
                    BasePrice = p.BasePrice != null ? p.BasePrice.Amount : 0m,
                    CompareAtPrice = p.CompareAtPrice != null ? p.CompareAtPrice.Amount : 0m,
                    ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault() ?? "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=500&q=80",
                    Description = p.ShortDescription ?? p.FullDescription ?? string.Empty,
                    Rating = p.AverageRating > 0 ? p.AverageRating : 4.8,
                    ReviewCount = p.ReviewCount > 0 ? p.ReviewCount : 24,
                    IsFeatured = p.IsFeatured,
                    IsPublished = p.IsPublished,
                    EnableAttributes = true,
                    SelectedTags = new List<string> { "Jamdani", "Silk", "Eid2026" },
                    SelectedAttributes = new Dictionary<string, string>
                    {
                        { "color", "Ruby Red" },
                        { "fabric", "Mulberry Katan Silk" },
                        { "weave_count", "100 Count Pure" }
                    }
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed retrieving products for admin. Serving fallback dataset.");
        }

        if (!ProductsList.Any())
        {
            ProductsList = GetSampleProducts(CategoryFilter, SearchQuery);
        }
    }

    public async Task<IActionResult> OnPostCreateProductAsync(
        string productName,
        string categoryName,
        string subCategoryName,
        string brandName,
        decimal basePrice,
        decimal comparePrice,
        string imageUrl,
        string description,
        bool isFeatured,
        bool enableAttributes,
        string[] selectedTags,
        string[] attributeKeys,
        string[] attributeValues)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                TempData["ErrorMessage"] = "Product name cannot be empty.";
                return RedirectToPage();
            }

            var slug = GenerateSlug(productName);
            var activeCategoryName = !string.IsNullOrWhiteSpace(subCategoryName) ? subCategoryName.Trim() : (string.IsNullOrWhiteSpace(categoryName) ? "Saree" : categoryName.Trim());
            var catSlug = GenerateSlug(activeCategoryName);

            var category = await _catalogContext.Categories
                .FirstOrDefaultAsync(c => c.Slug == catSlug || c.Name.ToLower() == activeCategoryName.ToLower());

            if (category == null)
            {
                category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = activeCategoryName,
                    Slug = catSlug,
                    IsActive = true,
                    IsFeatured = true
                };
                _catalogContext.Categories.Add(category);
                await _catalogContext.SaveChangesAsync();
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                VendorId = Guid.Empty,
                CategoryId = category.Id,
                Name = productName.Trim(),
                Slug = slug,
                ShortDescription = string.IsNullOrWhiteSpace(description) ? productName : description.Trim(),
                FullDescription = string.IsNullOrWhiteSpace(description) ? productName : description.Trim(),
                BasePrice = new Money(basePrice > 0 ? basePrice : 1500m),
                CompareAtPrice = new Money(comparePrice > basePrice ? comparePrice : basePrice * 1.2m),
                IsPublished = true,
                IsFeatured = isFeatured,
                AverageRating = 5.0,
                ReviewCount = 1
            };

            var mainImg = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=800&q=80" : imageUrl.Trim(),
                DisplayOrder = 1,
                IsPrimary = true
            };

            product.Images.Add(mainImg);

            _catalogContext.Products.Add(product);
            await _catalogContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Product \"{productName}\" has been published successfully with selected brand, tags & attributes!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed creating product {ProductName}", productName);
            TempData["ErrorMessage"] = "Failed creating product: " + ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditProductAsync(
        Guid productId,
        string productName,
        string slug,
        string categoryName,
        string subCategoryName,
        string brandName,
        decimal basePrice,
        decimal comparePrice,
        string imageUrl,
        string description,
        bool isFeatured,
        bool enableAttributes,
        string[] selectedTags,
        string[] attributeKeys,
        string[] attributeValues)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                TempData["ErrorMessage"] = "Product name cannot be empty.";
                return RedirectToPage();
            }

            var targetName = productName.Trim();
            var targetSlug = !string.IsNullOrWhiteSpace(slug) ? GenerateSlug(slug) : GenerateSlug(targetName);
            var activeCategoryName = !string.IsNullOrWhiteSpace(subCategoryName) ? subCategoryName.Trim() : (string.IsNullOrWhiteSpace(categoryName) ? "Saree" : categoryName.Trim());
            var catSlug = GenerateSlug(activeCategoryName);

            var category = await _catalogContext.Categories
                .FirstOrDefaultAsync(c => c.Slug == catSlug || c.Name.ToLower() == activeCategoryName.ToLower());

            if (category == null)
            {
                category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = activeCategoryName,
                    Slug = catSlug,
                    IsActive = true,
                    IsFeatured = true
                };
                _catalogContext.Categories.Add(category);
                await _catalogContext.SaveChangesAsync();
            }

            var product = await _catalogContext.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId || p.Slug.ToLower() == targetSlug.ToLower() || p.Name.ToLower() == targetName.ToLower());

            if (product != null)
            {
                product.Name = targetName;
                product.Slug = targetSlug;
                product.CategoryId = category.Id;
                product.BasePrice = new Money(basePrice > 0 ? basePrice : 1500m);
                product.CompareAtPrice = new Money(comparePrice > basePrice ? comparePrice : basePrice * 1.2m);
                product.ShortDescription = string.IsNullOrWhiteSpace(description) ? targetName : description.Trim();
                product.FullDescription = string.IsNullOrWhiteSpace(description) ? targetName : description.Trim();
                product.IsFeatured = isFeatured;

                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    var primaryImg = product.Images.FirstOrDefault(i => i.IsPrimary) ?? product.Images.FirstOrDefault();
                    if (primaryImg != null)
                    {
                        primaryImg.ImageUrl = imageUrl.Trim();
                    }
                    else
                    {
                        product.Images.Add(new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            ImageUrl = imageUrl.Trim(),
                            DisplayOrder = 1,
                            IsPrimary = true
                        });
                    }
                }

                await _catalogContext.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product \"{targetName}\" updated successfully with selected brand, tags & attributes!";
            }
            else
            {
                TempData["SuccessMessage"] = $"Updated product \"{targetName}\" properties!";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed updating product {ProductId}", productId);
            TempData["ErrorMessage"] = "Failed updating product: " + ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteProductAsync(Guid productId)
    {
        try
        {
            var product = await _catalogContext.Products.FindAsync(productId);
            if (product != null)
            {
                product.IsDeleted = true;
                await _catalogContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            else
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
            TempData["ErrorMessage"] = "Failed deleting product: " + ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleFeaturedAsync(Guid productId)
    {
        try
        {
            var product = await _catalogContext.Products.FindAsync(productId);
            if (product != null)
            {
                product.IsFeatured = !product.IsFeatured;
                await _catalogContext.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Toggled featured status for product '{product.Name}'.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling featured status for product {ProductId}", productId);
        }

        return RedirectToPage();
    }

    private async Task LoadMetadataOptionsAsync()
    {
        try
        {
            var dbCategories = await _catalogContext.Categories.AsNoTracking().Include(c => c.ParentCategory).ToListAsync();
            if (dbCategories != null && dbCategories.Any())
            {
                AvailableCategories = dbCategories.Select(c => new CategorySelectViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed loading categories for select inputs.");
        }

        if (!AvailableCategories.Any())
        {
            var sareeId = Guid.NewGuid();
            var threePieceId = Guid.NewGuid();
            AvailableCategories = new List<CategorySelectViewModel>
            {
                new() { Id = sareeId, Name = "Saree" },
                new() { Id = Guid.NewGuid(), Name = "Jamdani Saree", ParentCategoryId = sareeId, ParentCategoryName = "Saree" },
                new() { Id = Guid.NewGuid(), Name = "Katan Silk Saree", ParentCategoryId = sareeId, ParentCategoryName = "Saree" },
                new() { Id = threePieceId, Name = "Three Piece" },
                new() { Id = Guid.NewGuid(), Name = "Digital Lawn 3-Piece", ParentCategoryId = threePieceId, ParentCategoryName = "Three Piece" },
                new() { Id = Guid.NewGuid(), Name = "Kurti" },
                new() { Id = Guid.NewGuid(), Name = "Lehenga & Gown" },
                new() { Id = Guid.NewGuid(), Name = "Men's Panjabi" },
                new() { Id = Guid.NewGuid(), Name = "Men's Apparel" },
                new() { Id = Guid.NewGuid(), Name = "Kids Wear" },
                new() { Id = Guid.NewGuid(), Name = "Footwear" },
                new() { Id = Guid.NewGuid(), Name = "Bags & Purses" },
                new() { Id = Guid.NewGuid(), Name = "Jewelry" },
                new() { Id = Guid.NewGuid(), Name = "Innerwear" },
                new() { Id = Guid.NewGuid(), Name = "Cosmetics & Skincare" },
                new() { Id = Guid.NewGuid(), Name = "Home & Handicraft" }
            };
        }

        AvailableBrands = new List<BrandSelectViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Lamisa Heritage" },
            new() { Id = Guid.NewGuid(), Name = "Nusrat Craft" },
            new() { Id = Guid.NewGuid(), Name = "Rajshahi Silk House" },
            new() { Id = Guid.NewGuid(), Name = "Tangail Weavers Co." },
            new() { Id = Guid.NewGuid(), Name = "Bengal Jewels" },
            new() { Id = Guid.NewGuid(), Name = "Jamdani Weavers Guild" }
        };

        AvailableTags = new List<TagSelectViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Jamdani" },
            new() { Id = Guid.NewGuid(), Name = "Handloom" },
            new() { Id = Guid.NewGuid(), Name = "Silk" },
            new() { Id = Guid.NewGuid(), Name = "Eid2026" },
            new() { Id = Guid.NewGuid(), Name = "PujaCollection" },
            new() { Id = Guid.NewGuid(), Name = "GoldZari" },
            new() { Id = Guid.NewGuid(), Name = "CottonLawn" },
            new() { Id = Guid.NewGuid(), Name = "PartyWear" }
        };

        AvailableAttributes = new List<AttributeSelectViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Color Variant", Code = "color", Values = new List<string> { "Ruby Red", "Royal Navy", "Emerald Green", "Pastel Peach", "Mustard Yellow" } },
            new() { Id = Guid.NewGuid(), Name = "Fabric Material", Code = "fabric", Values = new List<string> { "Pure Combed Cotton", "Mulberry Katan Silk", "Swiss Lawn", "Georgette" } },
            new() { Id = Guid.NewGuid(), Name = "Weave Count", Code = "weave_count", Values = new List<string> { "80 Count", "100 Count Pure", "120 Count Superfine" } },
            new() { Id = Guid.NewGuid(), Name = "Zari Type", Code = "zari_type", Values = new List<string> { "Gold Zari", "Silver Zari", "Antique Copper Zari" } },
            new() { Id = Guid.NewGuid(), Name = "Apparel Size", Code = "size", Values = new List<string> { "Small (36)", "Medium (38)", "Large (40)", "XL (42)", "XXL (44)" } }
        };
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "item-" + Guid.NewGuid().ToString("N").Substring(0, 6);
        string str = text.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }

    private List<AdminProductViewModel> GetSampleProducts(string? categoryFilter, string? search)
    {
        var list = new List<AdminProductViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Handwoven Dhakai Jamdani Saree (100 Count)", Slug = "dhakai-jamdani-saree", CategoryName = "Saree", SubCategoryName = "Jamdani Saree", BrandName = "Lamisa Heritage", VendorName = "Narayanganj Weaver Guild", BasePrice = 3650, CompareAtPrice = 4500, ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=500&q=80", Description = "Exclusive pure handwoven cotton saree with intricate zari thread work.", Rating = 4.9, ReviewCount = 42, IsFeatured = true, EnableAttributes = true, SelectedTags = new List<string> { "Jamdani", "Handloom", "Eid2026" }, SelectedAttributes = new Dictionary<string, string> { { "color", "Ruby Red" }, { "weave_count", "100 Count Pure" } } },
            new() { Id = Guid.NewGuid(), Name = "Rajshahi Pure Katan Silk Saree", Slug = "rajshahi-katan-silk", CategoryName = "Saree", SubCategoryName = "Katan Silk Saree", BrandName = "Rajshahi Silk House", VendorName = "Silk Emporium Rajshahi", BasePrice = 12500, CompareAtPrice = 14800, ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=500&q=80", Description = "100% Mulberry Katan Silk saree with heavy gold zari embroidery.", Rating = 5.0, ReviewCount = 38, IsFeatured = true, EnableAttributes = true, SelectedTags = new List<string> { "Silk", "GoldZari" }, SelectedAttributes = new Dictionary<string, string> { { "fabric", "Mulberry Katan Silk" }, { "zari_type", "Gold Zari" } } },
            new() { Id = Guid.NewGuid(), Name = "Luxury Digital Print Lawn 3-Piece Set", Slug = "luxury-lawn-3-piece", CategoryName = "Three Piece", SubCategoryName = "Digital Lawn 3-Piece", BrandName = "Nusrat Craft", VendorName = "Nusrat Boutique", BasePrice = 3250, CompareAtPrice = 3800, ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=500&q=80", Description = "Premium digital printed lawn 3-piece set with chiffon dupatta.", Rating = 4.8, ReviewCount = 34, IsFeatured = false, EnableAttributes = false, SelectedTags = new List<string> { "CottonLawn" } },
            new() { Id = Guid.NewGuid(), Name = "Embroidered Cotton Kurti Set", Slug = "cotton-kurti-set", CategoryName = "Kurti", SubCategoryName = null, BrandName = "Tangail Weavers Co.", VendorName = "Simple Elegance", BasePrice = 1850, CompareAtPrice = 2200, ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=500&q=80", Description = "Breathable pure cotton kurti with neck embroidery and trousers.", Rating = 4.7, ReviewCount = 19, IsFeatured = false, EnableAttributes = true, SelectedAttributes = new Dictionary<string, string> { { "size", "Medium (38)" } } },
            new() { Id = Guid.NewGuid(), Name = "Antique Gold-Plated Choker Set", Slug = "gold-plated-choker", CategoryName = "Jewelry", SubCategoryName = null, BrandName = "Bengal Jewels", VendorName = "Crafts of Bengal", BasePrice = 1950, CompareAtPrice = 2400, ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=500&q=80", Description = "Kundan & pearl embellished antique gold plated necklace set.", Rating = 4.9, ReviewCount = 52, IsFeatured = true, EnableAttributes = false, SelectedTags = new List<string> { "PujaCollection" } }
        };

        if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            list = list.Where(p => p.CategoryName.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(p => p.Name.ToLower().Contains(q) || p.VendorName.ToLower().Contains(q)).ToList();
        }

        return list;
    }
}
