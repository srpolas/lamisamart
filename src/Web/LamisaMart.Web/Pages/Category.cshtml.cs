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

        // 3. Ensure at least 5 products exist per category
        if (!Products.Any() || Products.Count < 5)
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

            case "lehenga-gown":
            case "lehenga":
            case "gown":
                CategoryName = "Lehenga & Evening Gowns";
                BengaliName = "লেহেঙ্গা ও গাউন";
                Description = "Royal bridal lehenga cholis, sequined party gowns, and traditional zardozi crafted festival wear.";
                BannerUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Lehengas", Slug = "", Icon = "bi-grid-fill", Count = 25 },
                    new() { Name = "Bridal Velvet", Slug = "bridal-lehenga", Icon = "bi-gem", Count = 10 },
                    new() { Name = "Evening Gowns", Slug = "gowns", Icon = "bi-stars", Count = 15 }
                };
                break;

            case "mens-panjabi":
            case "panjabi":
                CategoryName = "Men's Panjabi & Pajama Sets";
                BengaliName = "পাঞ্জাবি কালেকশন";
                Description = "Authentic Rajshahi silk, Dhakai Jamdani weave, and slub cotton designer Panjabis for Eid, Puja, and weddings.";
                BannerUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Panjabi", Slug = "", Icon = "bi-grid-fill", Count = 35 },
                    new() { Name = "Kabli & Slub Cotton", Slug = "kabli", Icon = "bi-person", Count = 15 },
                    new() { Name = "Silk Wedding Panjabi", Slug = "silk-panjabi", Icon = "bi-award", Count = 20 }
                };
                break;

            case "mens-apparel":
            case "mens-wear":
                CategoryName = "Men's Apparel & Formal Wear";
                BengaliName = "পুরুষদের পোশাক";
                Description = "Executive Oxford shirts, Slub cotton short kurtas, formal chino trousers, and casual polo t-shirts.";
                BannerUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Shirts & Pants", Slug = "", Icon = "bi-grid-fill", Count = 28 }
                };
                break;

            case "kids-wear":
            case "kids":
                CategoryName = "Kids Wear & Baby Fashion";
                BengaliName = "বাচ্চাদের পোশাক";
                Description = "Adorable party frocks, festive mini Panjabis, comfortable cotton suits, and kids wedding attire.";
                BannerUrl = "https://images.unsplash.com/photo-1519238263530-99bdd11df2ea?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Kids Wear", Slug = "", Icon = "bi-grid-fill", Count = 22 }
                };
                break;

            case "footwear":
            case "shoes":
                CategoryName = "Footwear & Traditional Shoes";
                BengaliName = "জুতা কালেকশন";
                Description = "Handcrafted Nagra Juttis, genuine leather loafers, pearl embroidered wedges, and comfortable sandals.";
                BannerUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Footwear", Slug = "", Icon = "bi-grid-fill", Count = 26 }
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

            case "home-handicraft":
            case "handicraft":
                CategoryName = "Home Decor & Handicrafts";
                BengaliName = "হোম ডেকর ও হস্তশিল্প";
                Description = "Traditional Nakshi Kantha quilts, antique brass tea sets, jute table runners, and terracotta wall hangings.";
                BannerUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=1400&q=80";
                SubCategories = new List<SubCategoryItem>
                {
                    new() { Name = "All Handicrafts", Slug = "", Icon = "bi-grid-fill", Count = 24 }
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

        switch (categorySlug)
        {
            case "saree":
            case "sarees":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Handwoven Dhakai Jamdani Saree (100 Count Pure Cotton)", Slug = "dhakai-jamdani-saree-100-count", VendorName = "Narayanganj Weaver Guild", CategoryName = "Saree", SubCategorySlug = "jamdani", Price = 6850, OriginalPrice = 8200, ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80", Rating = 4.9, ReviewCount = 42, IsNew = true, IsBestSeller = true, Material = "Pure Cotton & Gold Zari" },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Rajshahi Pure Katan Silk Saree with Heavy Zardozi Work", Slug = "rajshahi-katan-silk-saree", VendorName = "Silk Emporium Rajshahi", CategoryName = "Saree", SubCategorySlug = "katan", Price = 12500, OriginalPrice = 14800, ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80", Rating = 5.0, ReviewCount = 38, IsNew = false, IsBestSeller = true, Material = "100% Mulberry Silk" },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Floral Pastel Organza Party Saree with Unstitched Blouse", Slug = "floral-pastel-organza-saree", VendorName = "Nusrat Boutique", CategoryName = "Saree", SubCategorySlug = "organza", Price = 3950, OriginalPrice = 4500, ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80", Rating = 4.7, ReviewCount = 19, IsNew = true, IsBestSeller = false, Material = "Sheer Glass Organza" },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Tangail Handloom Soft Linen Cotton Daily Wear Saree", Slug = "tangail-handloom-linen-saree", VendorName = "Crafts of Bengal", CategoryName = "Saree", SubCategorySlug = "cotton", Price = 2450, OriginalPrice = 2900, ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80", Rating = 4.8, ReviewCount = 56, IsNew = false, IsBestSeller = true, Material = "Breathable Soft Cotton" },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Bridal Crimson Red Half-Silk Saree with Mirror & Cutwork", Slug = "bridal-crimson-red-half-silk-saree", VendorName = "Dhaka Heritage Saree", CategoryName = "Saree", SubCategorySlug = "bridal", Price = 8900, OriginalPrice = 10500, ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80", Rating = 4.9, ReviewCount = 27, IsNew = true, IsBestSeller = true, Material = "Half-Silk & Zari Thread" }
                });
                break;

            case "three-piece":
            case "three-pieces":
            case "3-piece":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Luxury Digital Print Pakistani Lawn 3-Piece Set", Slug = "luxury-lawn-3-piece-set", VendorName = "Nusrat Boutique", CategoryName = "Three Piece", SubCategorySlug = "lawn", Price = 3250, OriginalPrice = 3800, ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80", Rating = 4.8, ReviewCount = 34, IsNew = true, IsBestSeller = true, Material = "Swiss Lawn & Chiffon Dupatta" },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Embroidered Cotton Linen Salwar Kameez Suit", Slug = "embroidered-cotton-salwar-suit", VendorName = "Glamour Closet", CategoryName = "Three Piece", SubCategorySlug = "cotton-3p", Price = 2850, OriginalPrice = 3400, ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80", Rating = 4.7, ReviewCount = 22, IsNew = false, IsBestSeller = true, Material = "Pure Slub Cotton" },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Party Wear Organza 3-Piece with Heavy Pearl Dupatta", Slug = "party-organza-3-piece-pearl", VendorName = "Dhaka Heritage", CategoryName = "Three Piece", SubCategorySlug = "organza-3p", Price = 5400, OriginalPrice = 6200, ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80", Rating = 4.9, ReviewCount = 18, IsNew = true, IsBestSeller = false, Material = "Embroidered Organza" },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Designer Georgette Embroidered Anarkali 3-Piece Set", Slug = "designer-georgette-anarkali-3p", VendorName = "Simple Elegance", CategoryName = "Three Piece", SubCategorySlug = "lawn", Price = 4650, OriginalPrice = 5200, ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80", Rating = 4.8, ReviewCount = 29, IsNew = false, IsBestSeller = true, Material = "Faux Georgette" },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Swiss Voile Pure Cotton Unstitched 3-Piece Collection", Slug = "swiss-voile-unstitched-3p", VendorName = "Narayanganj Guild", CategoryName = "Three Piece", SubCategorySlug = "unstitched", Price = 2250, OriginalPrice = 2700, ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80", Rating = 4.6, ReviewCount = 15, IsNew = true, IsBestSeller = false, Material = "100% Swiss Voile" }
                });
                break;

            case "kurti":
            case "kurtis":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Premium Slub Cotton Printed Straight Kurti", Slug = "slub-cotton-straight-kurti", VendorName = "Nusrat Boutique", CategoryName = "Kurti", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80", Rating = 4.7, ReviewCount = 31, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Designer Silk Anarkali Kurti with Chiffon Dupatta", Slug = "designer-silk-anarkali-kurti", VendorName = "Glamour Closet", CategoryName = "Kurti", Price = 2850, OriginalPrice = 3300, ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80", Rating = 4.9, ReviewCount = 24, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Festive Embroidered Rayon Long Tunic Kurti", Slug = "festive-embroidered-rayon-kurti", VendorName = "Simple Elegance", CategoryName = "Kurti", Price = 1950, OriginalPrice = 2400, ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80", Rating = 4.6, ReviewCount = 19, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Casual Daily Denim Indigo Block Print Kurti", Slug = "casual-daily-indigo-kurti", VendorName = "Crafts of Bengal", CategoryName = "Kurti", Price = 1250, OriginalPrice = 1500, ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80", Rating = 4.5, ReviewCount = 42, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Hand-embroidered Chikankari White Cotton Kurti", Slug = "chikankari-white-cotton-kurti", VendorName = "Heritage Muslin", CategoryName = "Kurti", Price = 2200, OriginalPrice = 2600, ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80", Rating = 4.8, ReviewCount = 37, IsNew = true, IsBestSeller = true }
                });
                break;

            case "lehenga-gown":
            case "lehenga":
            case "gown":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Royal Crimson Velvet Bridal Lehenga Choli Set", Slug = "royal-crimson-velvet-bridal-lehenga", VendorName = "Rajshahi Silk House", CategoryName = "Lehenga & Gown", Price = 24500, OriginalPrice = 28000, ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80", Rating = 5.0, ReviewCount = 48, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Pastel Pink Net Party Wear Designer Lehenga", Slug = "pastel-pink-net-party-lehenga", VendorName = "Nusrat Boutique", CategoryName = "Lehenga & Gown", Price = 16800, OriginalPrice = 19500, ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80", Rating = 4.9, ReviewCount = 29, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Embroidered Georgette Floor-Length Anarkali Gown", Slug = "embroidered-georgette-anarkali-gown", VendorName = "Glamour Closet", CategoryName = "Lehenga & Gown", Price = 8500, OriginalPrice = 9800, ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80", Rating = 4.8, ReviewCount = 21, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Mirror-Work Silk Indo-Western Evening Gown", Slug = "mirror-work-silk-evening-gown", VendorName = "Simple Elegance", CategoryName = "Lehenga & Gown", Price = 11200, OriginalPrice = 13000, ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80", Rating = 4.7, ReviewCount = 14, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Organza Sequined Reception Lehenga Choli Set", Slug = "organza-sequined-reception-lehenga", VendorName = "Dhaka Heritage", CategoryName = "Lehenga & Gown", Price = 19500, OriginalPrice = 22500, ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80", Rating = 4.9, ReviewCount = 33, IsNew = false, IsBestSeller = true }
                });
                break;

            case "mens-panjabi":
            case "panjabi":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Designer Embroidery Kabli Slub Cotton Panjabi", Slug = "designer-embroidery-kabli-panjabi", VendorName = "Narayanganj Guild", CategoryName = "Men's Panjabi", Price = 2850, OriginalPrice = 3400, ImageUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=600&q=80", Rating = 4.9, ReviewCount = 52, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Rajshahi Silk Royal Wedding Panjabi & Pajama Set", Slug = "rajshahi-silk-royal-wedding-panjabi", VendorName = "Silk Emporium Rajshahi", CategoryName = "Men's Panjabi", Price = 6400, OriginalPrice = 7500, ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=600&q=80", Rating = 5.0, ReviewCount = 41, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Premium Linen Casual Summer Panjabi for Men", Slug = "premium-linen-casual-panjabi", VendorName = "Crafts of Bengal", CategoryName = "Men's Panjabi", Price = 1950, OriginalPrice = 2300, ImageUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=600&q=80", Rating = 4.7, ReviewCount = 28, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Dhakai Jamdani Weave Traditional White Panjabi", Slug = "dhakai-jamdani-weave-white-panjabi", VendorName = "Heritage Muslin", CategoryName = "Men's Panjabi", Price = 3650, OriginalPrice = 4200, ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=600&q=80", Rating = 4.8, ReviewCount = 36, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Festive Jacquard Silk Maroon Panjabi Suit", Slug = "festive-jacquard-silk-maroon-panjabi", VendorName = "Nusrat Boutique", CategoryName = "Men's Panjabi", Price = 4200, OriginalPrice = 4900, ImageUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=600&q=80", Rating = 4.8, ReviewCount = 22, IsNew = true, IsBestSeller = false }
                });
                break;

            case "mens-apparel":
            case "mens-wear":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Formal Oxford Slim-Fit Cotton Executive Shirt", Slug = "formal-oxford-slim-fit-shirt", VendorName = "Verified Executive", CategoryName = "Men's Apparel", Price = 1850, OriginalPrice = 2200, ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=600&q=80", Rating = 4.8, ReviewCount = 39, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Casual Denim Indigo Washed Jacket Shirt", Slug = "casual-denim-washed-jacket-shirt", VendorName = "Simple Elegance", CategoryName = "Men's Apparel", Price = 2450, OriginalPrice = 2900, ImageUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=600&q=80", Rating = 4.7, ReviewCount = 26, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Traditional Handloom Khadi Cotton Short Kurta", Slug = "traditional-khadi-cotton-short-kurta", VendorName = "Crafts of Bengal", CategoryName = "Men's Apparel", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=600&q=80", Rating = 4.6, ReviewCount = 18, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Premium Stretch Formal Chino Trousers for Men", Slug = "premium-stretch-chino-trousers", VendorName = "Glamour Closet", CategoryName = "Men's Apparel", Price = 2150, OriginalPrice = 2500, ImageUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=600&q=80", Rating = 4.7, ReviewCount = 31, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Handwoven Cotton Casual Summer Polo Shirt", Slug = "handwoven-cotton-summer-polo-shirt", VendorName = "Nusrat Boutique", CategoryName = "Men's Apparel", Price = 1250, OriginalPrice = 1500, ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=600&q=80", Rating = 4.5, ReviewCount = 20, IsNew = true, IsBestSeller = false }
                });
                break;

            case "kids-wear":
            case "kids":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Little Princess Embroidered Party Frock Set", Slug = "little-princess-embroidered-frock", VendorName = "Nusrat Kids", CategoryName = "Kids Wear", Price = 1650, OriginalPrice = 1950, ImageUrl = "https://images.unsplash.com/photo-1519238263530-99bdd11df2ea?w=600&q=80", Rating = 4.9, ReviewCount = 35, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Kids Cotton Festive Panjabi Pajama Set for Boys", Slug = "kids-cotton-festive-panjabi-set", VendorName = "Crafts of Bengal", CategoryName = "Kids Wear", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1621786062579-6125bf25208a?w=600&q=80", Rating = 4.8, ReviewCount = 29, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Baby Girl Cute Printed Cotton Dress with Hairband", Slug = "baby-girl-printed-cotton-dress", VendorName = "Simple Elegance", CategoryName = "Kids Wear", Price = 980, OriginalPrice = 1200, ImageUrl = "https://images.unsplash.com/photo-1519238263530-99bdd11df2ea?w=600&q=80", Rating = 4.7, ReviewCount = 18, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Junior Boys Formal Waistcoat & Shirt Set", Slug = "junior-boys-formal-waistcoat-set", VendorName = "Glamour Closet", CategoryName = "Kids Wear", Price = 1950, OriginalPrice = 2300, ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=600&q=80", Rating = 4.6, ReviewCount = 14, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Traditional Mini Jamdani Kids Festive Dress", Slug = "traditional-mini-jamdani-kids-dress", VendorName = "Narayanganj Guild", CategoryName = "Kids Wear", Price = 1850, OriginalPrice = 2200, ImageUrl = "https://images.unsplash.com/photo-1519238263530-99bdd11df2ea?w=600&q=80", Rating = 5.0, ReviewCount = 40, IsNew = true, IsBestSeller = true }
                });
                break;

            case "footwear":
            case "shoes":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Traditional Handcrafted Nagra Jutti for Women", Slug = "traditional-handcrafted-nagra-jutti", VendorName = "Bengal Footwear", CategoryName = "Footwear", Price = 1850, OriginalPrice = 2200, ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=600&q=80", Rating = 4.8, ReviewCount = 44, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Genuine Leather Formal Loafer Shoes for Men", Slug = "genuine-leather-formal-loafers", VendorName = "Crafts of Bengal", CategoryName = "Footwear", Price = 3450, OriginalPrice = 4000, ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=600&q=80", Rating = 4.9, ReviewCount = 37, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Pearl & Zardozi Embroidered Bridal Kolhapuri Wedges", Slug = "pearl-zardozi-bridal-kolhapuri-wedges", VendorName = "Nusrat Boutique", CategoryName = "Footwear", Price = 2650, OriginalPrice = 3100, ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=600&q=80", Rating = 4.7, ReviewCount = 19, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Casual Comfortable Slip-on Canvas Sneakers", Slug = "casual-comfortable-canvas-sneakers", VendorName = "Simple Elegance", CategoryName = "Footwear", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=600&q=80", Rating = 4.6, ReviewCount = 28, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Velvet Party Block Heels with Ankle Strap", Slug = "velvet-party-block-heels", VendorName = "Glamour Closet", CategoryName = "Footwear", Price = 2250, OriginalPrice = 2600, ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=600&q=80", Rating = 4.8, ReviewCount = 23, IsNew = true, IsBestSeller = true }
                });
                break;

            case "jewelry":
            case "jewellery":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Gold-Plated Kundan Bridal Choker Necklace Set", Slug = "gold-plated-kundan-bridal-necklace", VendorName = "Bengal Jewels", CategoryName = "Jewelry", Price = 4850, OriginalPrice = 5800, ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=600&q=80", Rating = 5.0, ReviewCount = 62, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Traditional Antique Brass Jhumka Earrings for Women", Slug = "traditional-antique-brass-jhumka", VendorName = "Crafts of Bengal", CategoryName = "Jewelry", Price = 950, OriginalPrice = 1200, ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=600&q=80", Rating = 4.8, ReviewCount = 49, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Silver Replica Oxidized Tribal Statement Necklace", Slug = "silver-replica-oxidized-tribal-necklace", VendorName = "Heritage Ornaments", CategoryName = "Jewelry", Price = 1650, OriginalPrice = 1950, ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=600&q=80", Rating = 4.7, ReviewCount = 21, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Pearl & Cubic Zirconia Designer Bangle Set (Set of 4)", Slug = "pearl-cz-designer-bangle-set", VendorName = "Nusrat Boutique", CategoryName = "Jewelry", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=600&q=80", Rating = 4.6, ReviewCount = 33, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Handcrafted Stone Studded Matha Patti & Tikka Set", Slug = "handcrafted-stone-matha-patti-tikka", VendorName = "Bengal Jewels", CategoryName = "Jewelry", Price = 1250, OriginalPrice = 1500, ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=600&q=80", Rating = 4.9, ReviewCount = 27, IsNew = true, IsBestSeller = true }
                });
                break;

            case "bags":
            case "handbags":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Royal Zardozi Velvet Party Clutch Bag", Slug = "royal-zardozi-velvet-clutch-bag", VendorName = "Nusrat Boutique", CategoryName = "Bags & Purses", Price = 1950, OriginalPrice = 2400, ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=600&q=80", Rating = 4.9, ReviewCount = 38, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Premium Genuine Leather Structured Ladies Tote Bag", Slug = "genuine-leather-structured-tote-bag", VendorName = "Crafts of Bengal", CategoryName = "Bags & Purses", Price = 3850, OriginalPrice = 4500, ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=600&q=80", Rating = 4.8, ReviewCount = 42, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Handcrafted Potli Bag with Pearl Tassels", Slug = "handcrafted-potli-bag-pearl-tassels", VendorName = "Simple Elegance", CategoryName = "Bags & Purses", Price = 1250, OriginalPrice = 1500, ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=600&q=80", Rating = 4.7, ReviewCount = 19, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Stylish Crossbody Canvas Travel Sling Bag", Slug = "stylish-crossbody-canvas-sling-bag", VendorName = "Glamour Closet", CategoryName = "Bags & Purses", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=600&q=80", Rating = 4.6, ReviewCount = 25, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Designer Synthetic Leather Shoulder Handbag", Slug = "designer-synthetic-leather-shoulder-bag", VendorName = "Nusrat Boutique", CategoryName = "Bags & Purses", Price = 2250, OriginalPrice = 2700, ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=600&q=80", Rating = 4.8, ReviewCount = 31, IsNew = true, IsBestSeller = true }
                });
                break;

            case "cosmetics":
            case "beauty":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Organic Kumkumadi Radiance Face Serum (30ml)", Slug = "organic-kumkumadi-radiance-serum", VendorName = "Botanical Beauty", CategoryName = "Cosmetics & Skincare", Price = 1650, OriginalPrice = 1950, ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=600&q=80", Rating = 4.9, ReviewCount = 54, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Matte Velvet Hydrating Long-Lasting Lipstick", Slug = "matte-velvet-hydrating-lipstick", VendorName = "Glamour Cosmetics", CategoryName = "Cosmetics & Skincare", Price = 850, OriginalPrice = 1100, ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=600&q=80", Rating = 4.8, ReviewCount = 47, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Pure Rosewater & Saffron Botanical Facial Toner", Slug = "pure-rosewater-saffron-facial-toner", VendorName = "Botanical Beauty", CategoryName = "Cosmetics & Skincare", Price = 750, OriginalPrice = 950, ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=600&q=80", Rating = 4.7, ReviewCount = 22, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Deep Nourishing Ayurvedic Hair Growth Oil (200ml)", Slug = "deep-nourishing-ayurvedic-hair-oil", VendorName = "Herbal Care BD", CategoryName = "Cosmetics & Skincare", Price = 1250, OriginalPrice = 1500, ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=600&q=80", Rating = 4.9, ReviewCount = 38, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Vitamin C Brightening Daily Moisturizer Cream", Slug = "vitamin-c-brightening-moisturizer-cream", VendorName = "Glamour Cosmetics", CategoryName = "Cosmetics & Skincare", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=600&q=80", Rating = 4.8, ReviewCount = 29, IsNew = true, IsBestSeller = false }
                });
                break;

            case "innerwear":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Premium Breathable Seamless Cotton Everyday Bra Set", Slug = "premium-seamless-cotton-bra-set", VendorName = "Comfort Lingerie", CategoryName = "Innerwear", Price = 950, OriginalPrice = 1200, ImageUrl = "https://images.unsplash.com/photo-1516762689617-e1cffcef479d?w=600&q=80", Rating = 4.8, ReviewCount = 32, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Soft Satin Silk Nightwear Robe & Gown Set", Slug = "soft-satin-silk-nightwear-robe-set", VendorName = "Glamour Closet", CategoryName = "Innerwear", Price = 1850, OriginalPrice = 2200, ImageUrl = "https://images.unsplash.com/photo-1516762689617-e1cffcef479d?w=600&q=80", Rating = 4.9, ReviewCount = 27, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "High-Waist Seamless Body Shaping Corset Shorts", Slug = "high-waist-seamless-body-shaper", VendorName = "Comfort Lingerie", CategoryName = "Innerwear", Price = 1250, OriginalPrice = 1500, ImageUrl = "https://images.unsplash.com/photo-1516762689617-e1cffcef479d?w=600&q=80", Rating = 4.7, ReviewCount = 18, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Ultra-Soft Organic Cotton Comfy Pajama Sleepwear", Slug = "ultra-soft-organic-cotton-pajamas", VendorName = "Simple Elegance", CategoryName = "Innerwear", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1516762689617-e1cffcef479d?w=600&q=80", Rating = 4.6, ReviewCount = 21, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Non-Padded Wireless Comfort T-Shirt Bra", Slug = "non-padded-wireless-t-shirt-bra", VendorName = "Comfort Lingerie", CategoryName = "Innerwear", Price = 850, OriginalPrice = 1050, ImageUrl = "https://images.unsplash.com/photo-1516762689617-e1cffcef479d?w=600&q=80", Rating = 4.8, ReviewCount = 29, IsNew = true, IsBestSeller = true }
                });
                break;

            case "home-handicraft":
            case "handicraft":
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Hand-blocked Nakshi Kantha Quilt (Pure Cotton King Size)", Slug = "hand-blocked-nakshi-kantha-quilt", VendorName = "Crafts of Bengal", CategoryName = "Home & Handicraft", Price = 4850, OriginalPrice = 5800, ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=600&q=80", Rating = 5.0, ReviewCount = 46, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Traditional Brass Antique Tea Set with Tray", Slug = "traditional-brass-antique-tea-set", VendorName = "Heritage Handicrafts", CategoryName = "Home & Handicraft", Price = 3650, OriginalPrice = 4200, ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=600&q=80", Rating = 4.9, ReviewCount = 31, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Jute Fiber Decorative Table Runner & Placemats", Slug = "jute-fiber-decorative-table-runner", VendorName = "Crafts of Bengal", CategoryName = "Home & Handicraft", Price = 1250, OriginalPrice = 1500, ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=600&q=80", Rating = 4.7, ReviewCount = 19, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Hand-Carved Wooden Jewelry Keepsake Box", Slug = "hand-carved-wooden-jewelry-box", VendorName = "Simple Elegance", CategoryName = "Home & Handicraft", Price = 1450, OriginalPrice = 1750, ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=600&q=80", Rating = 4.8, ReviewCount = 24, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = "Clay Terracotta Decorative Wall Hanging Plate Set", Slug = "clay-terracotta-decorative-wall-plates", VendorName = "Heritage Handicrafts", CategoryName = "Home & Handicraft", Price = 980, OriginalPrice = 1200, ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?w=600&q=80", Rating = 4.9, ReviewCount = 38, IsNew = true, IsBestSeller = true }
                });
                break;

            default:
                list.AddRange(new[]
                {
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = $"Premium {CategoryName} - Artisanal Edition 01", Slug = $"{categorySlug}-edition-01", VendorName = "Verified Artisan Vendor", CategoryName = CategoryName, Price = 2450, OriginalPrice = 2950, ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80", Rating = 4.8, ReviewCount = 19, IsNew = true, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = $"Exclusive {CategoryName} - Festive Style", Slug = $"{categorySlug}-festive-style", VendorName = "Nusrat Boutique", CategoryName = CategoryName, Price = 3850, OriginalPrice = 4400, ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=600&q=80", Rating = 4.9, ReviewCount = 28, IsNew = false, IsBestSeller = true },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = $"Handcrafted {CategoryName} Signature Piece", Slug = $"{categorySlug}-signature-piece", VendorName = "Crafts of Bengal", CategoryName = CategoryName, Price = 4950, OriginalPrice = 5800, ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80", Rating = 4.7, ReviewCount = 15, IsNew = true, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = $"Daily Wear {CategoryName} Comfort Edition", Slug = $"{categorySlug}-daily-comfort", VendorName = "Simple Elegance", CategoryName = CategoryName, Price = 1650, OriginalPrice = 1950, ImageUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=600&q=80", Rating = 4.6, ReviewCount = 42, IsNew = false, IsBestSeller = false },
                    new CategoryProductViewModel { Id = Guid.NewGuid(), Name = $"Designer {CategoryName} Masterpiece Collection", Slug = $"{categorySlug}-masterpiece", VendorName = "Dhaka Heritage", CategoryName = CategoryName, Price = 5600, OriginalPrice = 6400, ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=600&q=80", Rating = 5.0, ReviewCount = 31, IsNew = true, IsBestSeller = true }
                });
                break;
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
