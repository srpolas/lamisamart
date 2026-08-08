using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LamisaMart.Catalog.Application.DTOs;
using LamisaMart.Catalog.Application.Categories.Queries;
using LamisaMart.Catalog.Application.Products.Queries;

namespace LamisaMart.Web.Pages;

public class CategoryModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoryModel> _logger;

    public CategoryModel(IMediator mediator, ILogger<CategoryModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? Slug { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Sub { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public string CategoryName { get; set; } = "Saree Collection";
    public string BengaliName { get; set; } = "শাড়ি কালেকশন";
    public string Description { get; set; } = "";
    public string BannerUrl { get; set; } = "";
    public string ActiveSlug { get; set; } = "saree";

    public List<SubCategoryItem> SubCategories { get; set; } = new();
    public List<CategoryProductViewModel> Products { get; set; } = new();
    public int TotalProducts { get; set; }

    public class SubCategoryItem
    {
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Icon { get; set; } = "bi-tag";
        public int Count { get; set; }
    }

    public class CategoryProductViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string VendorName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int DiscountPercent => OriginalPrice > Price ? (int)Math.Round((1 - (Price / OriginalPrice)) * 100) : 0;
        public string ImageUrl { get; set; } = "";
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsNew { get; set; }
        public bool IsBestSeller { get; set; }
        public string SubCategorySlug { get; set; } = "";
        public string Material { get; set; } = "";
    }

    public async Task OnGetAsync()
    {
        ActiveSlug = string.IsNullOrWhiteSpace(Slug) ? "saree" : Slug.Trim().ToLower();

        // 1. Setup metadata & subcategories for the requested category
        SetupCategoryMetadata(ActiveSlug);

        // 2. Try fetching from database via MediatR
        try
        {
            var dbCategory = await _mediator.Send(new GetCategoryBySlugQuery(ActiveSlug));
            if (dbCategory != null)
            {
                CategoryName = dbCategory.Name;
                if (!string.IsNullOrEmpty(dbCategory.ImageUrl)) 
                    BannerUrl = dbCategory.ImageUrl;
            }

            var dbProducts = await _mediator.Send(new GetProductsByCategoryQuery(ActiveSlug, Sort, MinPrice, MaxPrice, Sub));

            if (dbProducts != null && dbProducts.Any())
            {
                Products = dbProducts.Select(p => new CategoryProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    VendorName = "Verified Vendor",
                    CategoryName = p.CategoryName,
                    Price = p.BasePriceAmount,
                    OriginalPrice = p.CompareAtPriceAmount > 0 ? p.CompareAtPriceAmount : p.BasePriceAmount,
                    ImageUrl = !string.IsNullOrEmpty(p.PrimaryImageUrl) ? p.PrimaryImageUrl : "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=500&q=80",
                    Rating = p.AverageRating > 0 ? p.AverageRating : 4.8,
                    ReviewCount = p.ReviewCount > 0 ? p.ReviewCount : 24,
                    IsNew = p.IsFeatured,
                    IsBestSeller = p.IsFeatured
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed retrieving products from DbContext. Falling back to curated catalog.");
        }

        // 3. Fallback to curated catalog list if DbContext has no records for this category yet
        if (!Products.Any())
        {
            Products = GetCuratedProductsForCategory(ActiveSlug, Sub, Sort, MinPrice, MaxPrice, Q);
        }

        TotalProducts = Products.Count;
    }

    private void SetupCategoryMetadata(string slug)
    {
        switch (slug)
        {
            case "saree":
            case "sarees":
                CategoryName = "Sarees & Ethnic Collection";
                BengaliName = "শাড়ি কালেকশন";
                Description = "Explore Bangladesh's finest handcrafted Jamdani, Katan, Silk, Muslin, Organza and Half-Silk sarees sourced directly from authentic weavers and top boutique vendors.";
                BannerUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Sarees", Slug = "", Icon = "bi-grid-fill", Count = 48 },
                    new() { Name = "Jamdani Saree", Slug = "jamdani", Icon = "bi-flower1", Count = 16 },
                    new() { Name = "Katan Silk", Slug = "katan", Icon = "bi-gem", Count = 12 },
                    new() { Name = "Organza Saree", Slug = "organza", Icon = "bi-stars", Count = 8 },
                    new() { Name = "Cotton & Linen", Slug = "cotton", Icon = "bi-sun", Count = 7 },
                    new() { Name = "Bridal & Party", Slug = "bridal", Icon = "bi-heart-fill", Count = 5 }
                };
                break;

            case "three-piece":
            case "three-pieces":
            case "3-piece":
                CategoryName = "Three-Piece & Salwar Kameez";
                BengaliName = "থ্রি-পিস কালেকশন";
                Description = "Premium lawn, embroidered cotton, georgette, and organza 3-piece sets designed for elegance, comfort, and festive occasions.";
                BannerUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All 3-Piece", Slug = "", Icon = "bi-grid-fill", Count = 36 },
                    new() { Name = "Luxury Lawn", Slug = "lawn", Icon = "bi-tsunami", Count = 14 },
                    new() { Name = "Cotton Embroidered", Slug = "cotton-3p", Icon = "bi-scissors", Count = 10 },
                    new() { Name = "Party Organza", Slug = "organza-3p", Icon = "bi-sparkles", Count = 8 },
                    new() { Name = "Unstitched Sets", Slug = "unstitched", Icon = "bi-card-text", Count = 4 }
                };
                break;

            case "kurti":
            case "kurtis":
                CategoryName = "Kurtis, Tunics & Tops";
                BengaliName = "কুর্তি কালেকশন";
                Description = "Stylish, comfortable, and versatile cotton, rayon, and silk kurtis for daily wear, office, and social gatherings.";
                BannerUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Kurtis", Slug = "", Icon = "bi-grid-fill", Count = 30 },
                    new() { Name = "Single Kurti", Slug = "single-kurti", Icon = "bi-person-standing", Count = 12 },
                    new() { Name = "Kurti Set with Pant", Slug = "kurti-set", Icon = "bi-layers", Count = 10 },
                    new() { Name = "Anarkali & Long", Slug = "anarkali", Icon = "bi-magic", Count = 8 }
                };
                break;

            case "jewelry":
            case "jewellery":
                CategoryName = "Jewelry & Ornaments";
                BengaliName = "গহনা কালেকশন";
                Description = "Exquisite gold-plated, Kundan, silver-replica, and handcrafted traditional ornaments for every celebration.";
                BannerUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Jewelry", Slug = "", Icon = "bi-grid-fill", Count = 40 },
                    new() { Name = "Necklaces & Chokers", Slug = "necklace", Icon = "bi-circle", Count = 15 },
                    new() { Name = "Earrings & Jhumkas", Slug = "earrings", Icon = "bi-heart", Count = 15 },
                    new() { Name = "Bangles & Bracelets", Slug = "bangles", Icon = "bi-record-circle", Count = 10 }
                };
                break;

            case "bags":
            case "handbags":
                CategoryName = "Handbags, Clutches & Purses";
                BengaliName = "ব্যাগ কালেকশন";
                Description = "Chic party clutches, structured leather tote bags, traditional embroidered purses, and casual crossbody bags.";
                BannerUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Bags", Slug = "", Icon = "bi-grid-fill", Count = 25 },
                    new() { Name = "Party Clutches", Slug = "clutches", Icon = "bi-bag-heart", Count = 10 },
                    new() { Name = "Handbags & Totes", Slug = "totes", Icon = "bi-bag-check", Count = 10 },
                    new() { Name = "Crossbody Purses", Slug = "crossbody", Icon = "bi-bag", Count = 5 }
                };
                break;

            case "cosmetics":
            case "beauty":
                CategoryName = "Cosmetics & Beauty Products";
                BengaliName = "কসমেটিকস ও বিউটি";
                Description = "100% authentic skincare, lipsticks, makeup sets, and organic beauty essential oils imported from top trusted brands.";
                BannerUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Cosmetics", Slug = "", Icon = "bi-grid-fill", Count = 32 },
                    new() { Name = "Lipsticks & Care", Slug = "lipsticks", Icon = "bi-palette", Count = 12 },
                    new() { Name = "Skincare Serums", Slug = "skincare", Icon = "bi-droplet", Count = 10 },
                    new() { Name = "Perfumes & Oils", Slug = "perfumes", Icon = "bi-flower2", Count = 10 }
                };
                break;

            case "innerwear":
                CategoryName = "Comfort & Lingerie Innerwear";
                BengaliName = "ইনারওয়্যার কালেকশন";
                Description = "Premium quality breathable cotton shapewear, sleepwear, and seamless lingerie designed for day-long comfort.";
                BannerUrl = "https://images.unsplash.com/photo-1516762689617-e1cffcef479d?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Innerwear", Slug = "", Icon = "bi-grid-fill", Count = 20 },
                    new() { Name = "Sleepwear & Robes", Slug = "sleepwear", Icon = "bi-moon-stars", Count = 10 },
                    new() { Name = "Shapewear & Camis", Slug = "shapewear", Icon = "bi-shield", Count = 10 }
                };
                break;

            case "new-arrival":
            case "new-arrivals":
                CategoryName = "New Arrivals & Season Specials";
                BengaliName = "নতুন কালেকশন";
                Description = "Freshly launched designs from Bangladesh's top independent fashion vendors, updated weekly.";
                BannerUrl = "https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All New Arrivals", Slug = "", Icon = "bi-sparkles", Count = 40 }
                };
                break;

            case "sale":
            case "clearance":
                CategoryName = "Special Clearance & Discount Sale";
                BengaliName = "স্পেশাল সেল";
                Description = "Unbeatable clearance discounts up to 50% OFF on premium sarees, 3-pieces, and jewelry.";
                BannerUrl = "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Sale Items", Slug = "", Icon = "bi-percent", Count = 35 }
                };
                break;

            default:
                CategoryName = $"{char.ToUpper(slug[0]) + slug[1..]} Collection";
                BengaliName = "পণ্য কালেকশন";
                Description = $"Browse our curated selection of high-quality {slug} products from verified Bangladeshi vendors.";
                BannerUrl = "https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Items", Slug = "", Icon = "bi-grid-fill", Count = 24 }
                };
                break;
        }
    }

    private List<CategoryProductViewModel> GetCuratedProductsForCategory(
        string categorySlug, string? subcategorySlug, string? sort, decimal? minPrice, decimal? maxPrice, string? search)
    {
        var list = new List<CategoryProductViewModel>();

        if (categorySlug is "saree" or "sarees")
        {
            list.AddRange(new[]
            {
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Handwoven Dhakai Jamdani Saree (100 Count Pure Cotton)",
                    Slug = "dhakai-jamdani-saree-100-count",
                    VendorName = "Narayanganj Weaver Guild",
                    CategoryName = "Saree",
                    SubCategorySlug = "jamdani",
                    Price = 6850,
                    OriginalPrice = 8200,
                    ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80",
                    Rating = 4.9,
                    ReviewCount = 42,
                    IsNew = true,
                    IsBestSeller = true,
                    Material = "Pure Cotton & Gold Zari"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Rajshahi Pure Katan Silk Saree with Heavy Zardozi Work",
                    Slug = "rajshahi-katan-silk-saree",
                    VendorName = "Silk Emporium Rajshahi",
                    CategoryName = "Saree",
                    SubCategorySlug = "katan",
                    Price = 12500,
                    OriginalPrice = 14800,
                    ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80",
                    Rating = 5.0,
                    ReviewCount = 38,
                    IsNew = false,
                    IsBestSeller = true,
                    Material = "100% Mulberry Silk"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Floral Pastel Organza Party Saree with Unstitched Blouse",
                    Slug = "floral-pastel-organza-saree",
                    VendorName = "Nusrat Boutique",
                    CategoryName = "Saree",
                    SubCategorySlug = "organza",
                    Price = 3950,
                    OriginalPrice = 4500,
                    ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80",
                    Rating = 4.7,
                    ReviewCount = 19,
                    IsNew = true,
                    IsBestSeller = false,
                    Material = "Sheer Glass Organza"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Tangail Handloom Soft Linen Cotton Daily Wear Saree",
                    Slug = "tangail-handloom-linen-saree",
                    VendorName = "Crafts of Bengal",
                    CategoryName = "Saree",
                    SubCategorySlug = "cotton",
                    Price = 2450,
                    OriginalPrice = 2900,
                    ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80",
                    Rating = 4.8,
                    ReviewCount = 56,
                    IsNew = false,
                    IsBestSeller = true,
                    Material = "Breathable Soft Cotton"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Bridal Crimson Red Half-Silk Saree with Mirror & Cutwork",
                    Slug = "bridal-crimson-red-half-silk-saree",
                    VendorName = "Dhaka Heritage Saree",
                    CategoryName = "Saree",
                    SubCategorySlug = "bridal",
                    Price = 8900,
                    OriginalPrice = 10500,
                    ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80",
                    Rating = 4.9,
                    ReviewCount = 27,
                    IsNew = true,
                    IsBestSeller = true,
                    Material = "Half-Silk & Zari Thread"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Designer Shimmering Georgette Sequence Party Saree",
                    Slug = "designer-georgette-sequence-saree",
                    VendorName = "Glamour Closet",
                    CategoryName = "Saree",
                    SubCategorySlug = "organza",
                    Price = 4750,
                    OriginalPrice = 5500,
                    ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80",
                    Rating = 4.6,
                    ReviewCount = 15,
                    IsNew = false,
                    IsBestSeller = false,
                    Material = "Chiffon Georgette"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Authentic Pure Muslin Hand-Painted Collector's Saree",
                    Slug = "authentic-muslin-hand-painted-saree",
                    VendorName = "Heritage Muslin BD",
                    CategoryName = "Saree",
                    SubCategorySlug = "jamdani",
                    Price = 15200,
                    OriginalPrice = 17500,
                    ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80",
                    Rating = 5.0,
                    ReviewCount = 12,
                    IsNew = true,
                    IsBestSeller = false,
                    Material = "Ultra-Fine Dhakai Muslin"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Traditional Cotton Hand-Block Print Saree with Tassels",
                    Slug = "traditional-cotton-block-print-saree",
                    VendorName = "Simple Elegance",
                    CategoryName = "Saree",
                    SubCategorySlug = "cotton",
                    Price = 1850,
                    OriginalPrice = 2200,
                    ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80",
                    Rating = 4.5,
                    ReviewCount = 31,
                    IsNew = false,
                    IsBestSeller = false,
                    Material = "100% Combed Cotton"
                }
            });
        }
        else if (categorySlug is "three-piece" or "three-pieces" or "3-piece")
        {
            list.AddRange(new[]
            {
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Luxury Digital Print Pakistani Lawn 3-Piece Set",
                    Slug = "luxury-lawn-3-piece-set",
                    VendorName = "Nusrat Boutique",
                    CategoryName = "Three Piece",
                    SubCategorySlug = "lawn",
                    Price = 3250,
                    OriginalPrice = 3800,
                    ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80",
                    Rating = 4.8,
                    ReviewCount = 34,
                    IsNew = true,
                    IsBestSeller = true,
                    Material = "Swiss Lawn & Chiffon Dupatta"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Embroidered Cotton Linen Salwar Kameez Suit",
                    Slug = "embroidered-cotton-salwar-suit",
                    VendorName = "Glamour Closet",
                    CategoryName = "Three Piece",
                    SubCategorySlug = "cotton-3p",
                    Price = 2850,
                    OriginalPrice = 3400,
                    ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80",
                    Rating = 4.7,
                    ReviewCount = 22,
                    IsNew = false,
                    IsBestSeller = true,
                    Material = "Pure Slub Cotton"
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Party Wear Organza 3-Piece with Heavy Pearl Dupatta",
                    Slug = "party-organza-3-piece-pearl",
                    VendorName = "Dhaka Heritage",
                    CategoryName = "Three Piece",
                    SubCategorySlug = "organza-3p",
                    Price = 5400,
                    OriginalPrice = 6200,
                    ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80",
                    Rating = 4.9,
                    ReviewCount = 18,
                    IsNew = true,
                    IsBestSeller = false,
                    Material = "Embroidered Organza"
                }
            });
        }
        else
        {
            // Generic sample products for all other categories
            list.AddRange(new[]
            {
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"Premium {CategoryName} - Edition 01",
                    Slug = $"{categorySlug}-edition-01",
                    VendorName = "Verified Premium Vendor",
                    CategoryName = CategoryName,
                    Price = 2450,
                    OriginalPrice = 2950,
                    ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80",
                    Rating = 4.8,
                    ReviewCount = 19,
                    IsNew = true,
                    IsBestSeller = true
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"Exclusive {CategoryName} - Festive Style",
                    Slug = $"{categorySlug}-festive-style",
                    VendorName = "Nusrat Boutique",
                    CategoryName = CategoryName,
                    Price = 3850,
                    OriginalPrice = 4400,
                    ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80",
                    Rating = 4.9,
                    ReviewCount = 28,
                    IsNew = false,
                    IsBestSeller = true
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"Handcrafted {CategoryName} Signature Designer Piece",
                    Slug = $"{categorySlug}-signature-piece",
                    VendorName = "Crafts of Bengal",
                    CategoryName = CategoryName,
                    Price = 4950,
                    OriginalPrice = 5800,
                    ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80",
                    Rating = 4.7,
                    ReviewCount = 15,
                    IsNew = true,
                    IsBestSeller = false
                },
                new CategoryProductViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"Daily Wear {CategoryName} Comfort Edition",
                    Slug = $"{categorySlug}-daily-comfort",
                    VendorName = "Simple Elegance",
                    CategoryName = CategoryName,
                    Price = 1650,
                    OriginalPrice = 1950,
                    ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80",
                    Rating = 4.6,
                    ReviewCount = 42,
                    IsNew = false,
                    IsBestSeller = false
                }
            });
        }

        // Apply Subcategory Filter
        if (!string.IsNullOrWhiteSpace(subcategorySlug))
        {
            var subFiltered = list.Where(p => p.SubCategorySlug.Equals(subcategorySlug, StringComparison.OrdinalIgnoreCase)).ToList();
            if (subFiltered.Any()) list = subFiltered;
        }

        // Apply Price Filter
        if (minPrice.HasValue) list = list.Where(p => p.Price >= minPrice.Value).ToList();
        if (maxPrice.HasValue) list = list.Where(p => p.Price <= maxPrice.Value).ToList();

        // Apply Search query
        if (!string.IsNullOrWhiteSpace(search))
        {
            var qLower = search.Trim().ToLower();
            list = list.Where(p => p.Name.ToLower().Contains(qLower) || p.VendorName.ToLower().Contains(qLower)).ToList();
        }

        // Apply Sorting
        list = sort?.ToLower() switch
        {
            "price-low" => list.OrderBy(p => p.Price).ToList(),
            "price-high" => list.OrderByDescending(p => p.Price).ToList(),
            "rating" => list.OrderByDescending(p => p.Rating).ToList(),
            "newest" => list.OrderByDescending(p => p.IsNew).ToList(),
            _ => list.OrderByDescending(p => p.IsBestSeller).ThenByDescending(p => p.Rating).ToList()
        };

        return list;
    }
}
