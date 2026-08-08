using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LamisaMart.PageBuilder.Application.Common.Interfaces;
using LamisaMart.PageBuilder.Domain.Entities;

namespace LamisaMart.Web.Pages.Admin;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class PageBuilderModel : PageModel
{
    private readonly IPageBuilderDbContext _pageBuilderContext;
    private readonly ILogger<PageBuilderModel> _logger;

    public PageBuilderModel(IPageBuilderDbContext pageBuilderContext, ILogger<PageBuilderModel> logger)
    {
        _pageBuilderContext = pageBuilderContext;
        _logger = logger;
    }

    [BindProperty]
    public PageLayoutViewModel CurrentPage { get; set; } = new();

    [BindProperty]
    public string SectionsJsonPayload { get; set; } = "[]";

    public class PageLayoutViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "Main Home Page v1";
        public string Route { get; set; } = "/";
        public bool IsActive { get; set; } = true;
        public DateTime? PublishedAt { get; set; }
        public List<SectionViewModel> Sections { get; set; } = new();
    }

    public class SectionViewModel
    {
        public Guid Id { get; set; }
        public string SectionType { get; set; } = "HeroBanner";
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; } = true;
        public string ContentPayloadJson { get; set; } = "{}";
    }

    public async Task OnGetAsync()
    {
        try
        {
            var layout = await _pageBuilderContext.PageLayouts
                .Include(p => p.Sections)
                .FirstOrDefaultAsync(p => p.Route == "/");

            if (layout != null)
            {
                CurrentPage = new PageLayoutViewModel
                {
                    Id = layout.Id,
                    Name = layout.Name,
                    Route = layout.Route,
                    IsActive = layout.IsActive,
                    PublishedAt = layout.PublishedAt,
                    Sections = layout.Sections
                        .OrderBy(s => s.SortOrder)
                        .Select(s => new SectionViewModel
                        {
                            Id = s.Id,
                            SectionType = s.SectionType,
                            SortOrder = s.SortOrder,
                            IsVisible = s.IsVisible,
                            ContentPayloadJson = s.ContentPayloadJson
                        }).ToList()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load page layout from DbContext. Returning fallback layout.");
        }

        if (CurrentPage.Sections == null || !CurrentPage.Sections.Any())
        {
            CurrentPage = GetDefaultHomePageLayout();
        }

        SectionsJsonPayload = JsonSerializer.Serialize(CurrentPage.Sections);
    }

    public async Task<IActionResult> OnPostPublishAsync()
    {
        try
        {
            var layout = await _pageBuilderContext.PageLayouts
                .Include(p => p.Sections)
                .FirstOrDefaultAsync(p => p.Route == "/");

            if (layout == null)
            {
                layout = new PageLayout
                {
                    Id = Guid.NewGuid(),
                    Name = CurrentPage.Name,
                    Route = "/",
                    Type = PageType.Home,
                    IsActive = CurrentPage.IsActive,
                    PublishedAt = DateTime.UtcNow
                };
                _pageBuilderContext.PageLayouts.Add(layout);
            }
            else
            {
                layout.Name = CurrentPage.Name;
                layout.IsActive = CurrentPage.IsActive;
                layout.PublishedAt = DateTime.UtcNow;
            }

            // Parse submitted sections payload
            if (!string.IsNullOrWhiteSpace(SectionsJsonPayload))
            {
                var submittedSections = JsonSerializer.Deserialize<List<SectionViewModel>>(SectionsJsonPayload);
                if (submittedSections != null)
                {
                    // Remove existing sections
                    _pageBuilderContext.PageSections.RemoveRange(layout.Sections);

                    // Add updated sections
                    int order = 1;
                    foreach (var s in submittedSections)
                    {
                        layout.Sections.Add(new PageSection
                        {
                            Id = Guid.NewGuid(),
                            PageLayoutId = layout.Id,
                            SectionType = s.SectionType,
                            SortOrder = order++,
                            IsVisible = s.IsVisible,
                            ContentPayloadJson = s.ContentPayloadJson
                        });
                    }
                }
            }

            await _pageBuilderContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "HomePage layout updated & published successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish page layout");
            TempData["ErrorMessage"] = "Failed to save page layout: " + ex.Message;
        }

        return RedirectToPage();
    }

    private PageLayoutViewModel GetDefaultHomePageLayout()
    {
        return new PageLayoutViewModel
        {
            Id = Guid.NewGuid(),
            Name = "Main Storefront Home Page",
            Route = "/",
            IsActive = true,
            PublishedAt = DateTime.UtcNow,
            Sections = new List<SectionViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    SectionType = "HeroBanner",
                    SortOrder = 1,
                    IsVisible = true,
                    ContentPayloadJson = JsonSerializer.Serialize(new
                    {
                        badgeText = "✨ Exclusive Eid & Puja Collection 2026",
                        title = "প্রতিদিনের সাজে, লমিসা মার্ট।",
                        subtitle = "Everyday Elegance, Authentic Bangladeshi Craftsmanship.",
                        buttonText = "SHOP NEW SAREES",
                        buttonLink = "/category/saree",
                        imageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=1200&q=80"
                    })
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    SectionType = "TrustBadges",
                    SortOrder = 2,
                    IsVisible = true,
                    ContentPayloadJson = JsonSerializer.Serialize(new
                    {
                        badge1 = "100% Authentic Handloom",
                        badge2 = "Cash on Delivery Nationwide",
                        badge3 = "Fast 2-3 Days Delivery",
                        badge4 = "7 Days Easy Returns"
                    })
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    SectionType = "CategoryGrid",
                    SortOrder = 3,
                    IsVisible = true,
                    ContentPayloadJson = JsonSerializer.Serialize(new
                    {
                        title = "Shop By Category",
                        subtitle = "Curated luxury sarees, 3-pieces, kurtis, & traditional jewelry",
                        categories = new[] { "Saree", "Three Piece", "Kurti", "Jewelry" }
                    })
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    SectionType = "PromotionalRow",
                    SortOrder = 4,
                    IsVisible = true,
                    ContentPayloadJson = JsonSerializer.Serialize(new
                    {
                        bannerTitle = "Rajshahi Silk & Jamdani Mega Sale",
                        discountText = "UP TO 35% OFF",
                        buttonText = "EXPLORE COLLECTION",
                        buttonLink = "/category/saree",
                        bgGradient = "linear-gradient(135deg, #1E1B4B 0%, #431407 100%)"
                    })
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    SectionType = "ProductCarousel",
                    SortOrder = 5,
                    IsVisible = true,
                    ContentPayloadJson = JsonSerializer.Serialize(new
                    {
                        title = "Featured Artisan Sarees",
                        categorySlug = "saree",
                        displayCount = 8
                    })
                }
            }
        };
    }
}
