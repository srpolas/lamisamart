using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.UICustomization;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class HeaderModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public HeaderModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public static HeaderSettingsStore Settings { get; set; } = new();

    public class HeaderSettingsStore
    {
        public string HeaderStyle { get; set; } = "Classic";
        public bool StickyHeader { get; set; } = true;
        public bool EnableTopBanner { get; set; } = true;
        public string TopBannerText { get; set; } = "🎉 Free Shipping on Orders Over ৳5,000 | Fast Delivery Across Bangladesh!";
        public string TopBannerBgColor { get; set; } = "#E11D48";
        public string TopBannerTextColor { get; set; } = "#FFFFFF";

        // Logo & Branding Controls
        public string LogoUrl { get; set; } = "/images/logo.png";
        public string FooterLogoUrl { get; set; } = "/images/logo-footer.png";
        public string FaviconUrl { get; set; } = "/favicon.ico";
        public string SiteTitleImageUrl { get; set; } = "/images/LamisaMart_design_concept.png";
        public string SiteName { get; set; } = "LAMISA MART";
        public string SiteBengaliName { get; set; } = "লমিসা মার্ট";

        public bool EnableSearchBar { get; set; } = true;
        public bool EnableInstantAutoComplete { get; set; } = true;
        public bool ShowWishlistIcon { get; set; } = true;
        public bool ShowCartDrawer { get; set; } = true;
    }

    [BindProperty] public string HeaderStyle { get; set; } = string.Empty;
    [BindProperty] public bool StickyHeader { get; set; } = true;
    [BindProperty] public bool EnableTopBanner { get; set; } = true;
    [BindProperty] public string TopBannerText { get; set; } = string.Empty;
    [BindProperty] public string TopBannerBgColor { get; set; } = string.Empty;
    [BindProperty] public string TopBannerTextColor { get; set; } = string.Empty;
    [BindProperty] public string LogoUrl { get; set; } = string.Empty;
    [BindProperty] public string FooterLogoUrl { get; set; } = string.Empty;
    [BindProperty] public string FaviconUrl { get; set; } = string.Empty;
    [BindProperty] public string SiteTitleImageUrl { get; set; } = string.Empty;
    [BindProperty] public string SiteName { get; set; } = string.Empty;
    [BindProperty] public string SiteBengaliName { get; set; } = string.Empty;
    [BindProperty] public bool EnableSearchBar { get; set; } = true;
    [BindProperty] public bool EnableInstantAutoComplete { get; set; } = true;
    [BindProperty] public bool ShowWishlistIcon { get; set; } = true;
    [BindProperty] public bool ShowCartDrawer { get; set; } = true;

    // File Upload Bindings
    [BindProperty] public IFormFile? LogoFile { get; set; }
    [BindProperty] public IFormFile? FaviconFile { get; set; }
    [BindProperty] public IFormFile? SiteTitleFile { get; set; }

    public void OnGet()
    {
        HeaderStyle = Settings.HeaderStyle;
        StickyHeader = Settings.StickyHeader;
        EnableTopBanner = Settings.EnableTopBanner;
        TopBannerText = Settings.TopBannerText;
        TopBannerBgColor = Settings.TopBannerBgColor;
        TopBannerTextColor = Settings.TopBannerTextColor;
        LogoUrl = Settings.LogoUrl;
        FooterLogoUrl = Settings.FooterLogoUrl;
        FaviconUrl = Settings.FaviconUrl;
        SiteTitleImageUrl = Settings.SiteTitleImageUrl;
        SiteName = Settings.SiteName;
        SiteBengaliName = Settings.SiteBengaliName;
        EnableSearchBar = Settings.EnableSearchBar;
        EnableInstantAutoComplete = Settings.EnableInstantAutoComplete;
        ShowWishlistIcon = Settings.ShowWishlistIcon;
        ShowCartDrawer = Settings.ShowCartDrawer;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "branding");
        Directory.CreateDirectory(uploadsFolder);

        // Upload Logo Image File
        if (LogoFile != null && LogoFile.Length > 0)
        {
            var fileName = $"logo_{Guid.NewGuid():N}{Path.GetExtension(LogoFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await LogoFile.CopyToAsync(stream);
            }
            LogoUrl = $"/uploads/branding/{fileName}";
        }

        // Upload Favicon Icon File
        if (FaviconFile != null && FaviconFile.Length > 0)
        {
            var fileName = $"favicon_{Guid.NewGuid():N}{Path.GetExtension(FaviconFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await FaviconFile.CopyToAsync(stream);
            }
            FaviconUrl = $"/uploads/branding/{fileName}";
        }

        // Upload Site Title Banner Image File
        if (SiteTitleFile != null && SiteTitleFile.Length > 0)
        {
            var fileName = $"banner_{Guid.NewGuid():N}{Path.GetExtension(SiteTitleFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await SiteTitleFile.CopyToAsync(stream);
            }
            SiteTitleImageUrl = $"/uploads/branding/{fileName}";
        }

        Settings.HeaderStyle = HeaderStyle ?? "Classic";
        Settings.StickyHeader = StickyHeader;
        Settings.EnableTopBanner = EnableTopBanner;
        Settings.TopBannerText = TopBannerText ?? string.Empty;
        Settings.TopBannerBgColor = TopBannerBgColor ?? "#E11D48";
        Settings.TopBannerTextColor = TopBannerTextColor ?? "#FFFFFF";
        Settings.LogoUrl = LogoUrl ?? string.Empty;
        Settings.FooterLogoUrl = FooterLogoUrl ?? string.Empty;
        Settings.FaviconUrl = FaviconUrl ?? "/favicon.ico";
        Settings.SiteTitleImageUrl = SiteTitleImageUrl ?? string.Empty;
        Settings.SiteName = SiteName ?? "LAMISA MART";
        Settings.SiteBengaliName = SiteBengaliName ?? "লমিসা মার্ট";
        Settings.EnableSearchBar = EnableSearchBar;
        Settings.EnableInstantAutoComplete = EnableInstantAutoComplete;
        Settings.ShowWishlistIcon = ShowWishlistIcon;
        Settings.ShowCartDrawer = ShowCartDrawer;

        TempData["SuccessMessage"] = "Site Logo, Favicon, Site Title Image & Header Settings uploaded and published live!";
        return RedirectToPage();
    }
}
