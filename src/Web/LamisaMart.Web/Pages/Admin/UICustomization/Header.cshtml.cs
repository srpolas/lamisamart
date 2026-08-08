using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.UICustomization;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class HeaderModel : PageModel
{
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

    public IActionResult OnPost()
    {
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

        TempData["SuccessMessage"] = "Site Logo, Favicon, Site Title Image & Header Settings saved and published live!";
        return RedirectToPage();
    }
}
