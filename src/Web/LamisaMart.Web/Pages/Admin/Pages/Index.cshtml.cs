using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Pages;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class IndexModel : PageModel
{
    private static readonly List<SystemPageViewModel> MasterPagesStore = InitMasterPagesStore();

    public List<SystemPageViewModel> PagesList { get; set; } = new();

    public class SystemPageViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public bool ShowInHeader { get; set; }
        public bool ShowInFooter { get; set; } = true;
        public string Status { get; set; } = "Published"; // Published / Draft
        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public int ViewsCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public void OnGet()
    {
        PagesList = MasterPagesStore.OrderBy(p => p.Route).ToList();
    }

    public IActionResult OnPostCreatePage(
        string title,
        string route,
        string contentHtml,
        bool showInHeader,
        bool showInFooter,
        string status,
        string metaTitle,
        string metaDescription)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(route))
        {
            TempData["ErrorMessage"] = "Page title and route URL are required.";
            return RedirectToPage();
        }

        var normalizedRoute = route.Trim();
        if (!normalizedRoute.StartsWith("/"))
        {
            normalizedRoute = "/" + normalizedRoute;
        }

        var newPage = new SystemPageViewModel
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Route = normalizedRoute,
            Slug = normalizedRoute.TrimStart('/'),
            ContentHtml = contentHtml ?? string.Empty,
            ShowInHeader = showInHeader,
            ShowInFooter = showInFooter,
            Status = !string.IsNullOrWhiteSpace(status) ? status : "Published",
            MetaTitle = !string.IsNullOrWhiteSpace(metaTitle) ? metaTitle : title,
            MetaDescription = metaDescription ?? string.Empty,
            ViewsCount = 0,
            UpdatedAt = DateTime.UtcNow
        };

        MasterPagesStore.Add(newPage);
        TempData["SuccessMessage"] = $"New page '{title.Trim()}' created successfully!";
        return RedirectToPage();
    }

    public IActionResult OnPostEditPage(
        Guid id,
        string title,
        string route,
        string contentHtml,
        bool showInHeader,
        bool showInFooter,
        string status,
        string metaTitle,
        string metaDescription)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(route))
        {
            TempData["ErrorMessage"] = "Page title and route URL are required.";
            return RedirectToPage();
        }

        var targetPage = MasterPagesStore.FirstOrDefault(p => p.Id == id);
        if (targetPage != null)
        {
            var normalizedRoute = route.Trim();
            if (!normalizedRoute.StartsWith("/"))
            {
                normalizedRoute = "/" + normalizedRoute;
            }

            targetPage.Title = title.Trim();
            targetPage.Route = normalizedRoute;
            targetPage.Slug = normalizedRoute.TrimStart('/');
            targetPage.ContentHtml = contentHtml ?? string.Empty;
            targetPage.ShowInHeader = showInHeader;
            targetPage.ShowInFooter = showInFooter;
            targetPage.Status = status;
            targetPage.MetaTitle = metaTitle;
            targetPage.MetaDescription = metaDescription;
            targetPage.UpdatedAt = DateTime.UtcNow;
        }

        TempData["SuccessMessage"] = $"Page '{title.Trim()}' updated successfully!";
        return RedirectToPage();
    }

    public IActionResult OnPostToggleStatus(Guid id)
    {
        var targetPage = MasterPagesStore.FirstOrDefault(p => p.Id == id);
        if (targetPage != null)
        {
            targetPage.Status = targetPage.Status == "Published" ? "Draft" : "Published";
            targetPage.UpdatedAt = DateTime.UtcNow;
            TempData["SuccessMessage"] = $"Page '{targetPage.Title}' status changed to {targetPage.Status}!";
        }
        return RedirectToPage();
    }

    public IActionResult OnPostDeletePage(Guid id)
    {
        var targetPage = MasterPagesStore.FirstOrDefault(p => p.Id == id);
        if (targetPage != null)
        {
            MasterPagesStore.Remove(targetPage);
            TempData["SuccessMessage"] = $"Page '{targetPage.Title}' deleted successfully!";
        }
        return RedirectToPage();
    }

    private static List<SystemPageViewModel> InitMasterPagesStore()
    {
        var now = DateTime.UtcNow;
        return new List<SystemPageViewModel>
        {
            new()
            {
                Id = Guid.Parse("ba111111-1111-1111-1111-111111111111"),
                Title = "Home Storefront Page",
                Slug = "",
                Route = "/",
                ContentHtml = "<h3>Welcome to LamisaMart Bangladesh</h3><p>Discover handcrafted Dhakai Jamdani, Rajshahi Katan Silk, and traditional Bangladeshi fashion.</p>",
                ShowInHeader = true,
                ShowInFooter = true,
                Status = "Published",
                MetaTitle = "LamisaMart — Premier Bangladeshi Handloom & Fashion E-Commerce",
                MetaDescription = "Buy authentic Dhakai Jamdani, Katan Silk, Three-Piece Suits, and Artisan Jewelry online in Bangladesh.",
                ViewsCount = 48920,
                UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = Guid.Parse("ba222222-2222-2222-2222-222222222222"),
                Title = "Privacy Policy",
                Slug = "privacy",
                Route = "/privacy",
                ContentHtml = "<h2>Privacy Policy</h2><p>LamisaMart is committed to safeguarding your personal data, payment info, and browsing security.</p>",
                ShowInHeader = false,
                ShowInFooter = true,
                Status = "Published",
                MetaTitle = "Privacy Policy — LamisaMart Bangladesh",
                MetaDescription = "Learn how LamisaMart collects, uses, and protects customer personal data.",
                ViewsCount = 1420,
                UpdatedAt = now.AddDays(-5)
            },
            new()
            {
                Id = Guid.Parse("ba333333-3333-3333-3333-333333333333"),
                Title = "Terms of Service",
                Slug = "terms",
                Route = "/terms",
                ContentHtml = "<h2>Terms of Service</h2><p>By placing an order on LamisaMart, you agree to our purchasing terms, payment verification, and delivery policies.</p>",
                ShowInHeader = false,
                ShowInFooter = true,
                Status = "Published",
                MetaTitle = "Terms of Service — LamisaMart Bangladesh",
                MetaDescription = "Official terms and conditions governing purchases, refunds, and user accounts.",
                ViewsCount = 980,
                UpdatedAt = now.AddDays(-8)
            },
            new()
            {
                Id = Guid.Parse("ba444444-4444-4444-4444-444444444444"),
                Title = "About Us & Artisan Heritage",
                Slug = "about",
                Route = "/about",
                ContentHtml = "<h2>Our Artisan Story</h2><p>Connecting traditional Bangladeshi weavers from Narayanganj, Tangail, and Rajshahi directly with fashion enthusiasts worldwide.</p>",
                ShowInHeader = true,
                ShowInFooter = true,
                Status = "Published",
                MetaTitle = "About Us — LamisaMart Heritage & Artisans",
                MetaDescription = "Learn about LamisaMart's mission to preserve traditional Bangladeshi weaving craftsmanship.",
                ViewsCount = 3150,
                UpdatedAt = now.AddDays(-12)
            },
            new()
            {
                Id = Guid.Parse("ba555555-5555-5555-5555-555555555555"),
                Title = "Customer Support & Contact",
                Slug = "contact",
                Route = "/contact",
                ContentHtml = "<h2>Get in Touch</h2><p>Helpline: +880 9610-123456 | Email: support@lamisamart.bd | Headquarters: Malibagh, Dhaka.</p>",
                ShowInHeader = true,
                ShowInFooter = true,
                Status = "Published",
                MetaTitle = "Contact Us — LamisaMart Helpline & Outlets",
                MetaDescription = "Customer support hotline, office locations, and inquiry form.",
                ViewsCount = 5600,
                UpdatedAt = now.AddDays(-3)
            },
            new()
            {
                Id = Guid.Parse("ba666666-6666-6666-6666-666666666666"),
                Title = "Shipping & Nationwide Delivery Policy",
                Slug = "shipping",
                Route = "/shipping",
                ContentHtml = "<h2>Nationwide Delivery</h2><p>Inside Dhaka: ৳60 (24-48 hours). Outside Dhaka: ৳120 via Pathao / SteadFast (3-5 days).</p>",
                ShowInHeader = false,
                ShowInFooter = true,
                Status = "Published",
                MetaTitle = "Shipping Policy — LamisaMart Delivery Charges",
                MetaDescription = "Delivery timescales, shipping fees, and tracking information across Bangladesh.",
                ViewsCount = 2890,
                UpdatedAt = now.AddDays(-4)
            }
        };
    }
}
