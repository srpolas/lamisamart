using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.UICustomization;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class FooterModel : PageModel
{
    public static FooterSettingsStore StoreSettings { get; set; } = new();

    public class FooterSettingsStore
    {
        public string FooterAboutText { get; set; } = "Premium Bangladeshi multi-vendor marketplace for women's fashion, clothing, accessories, and beauty products.";
        public string SupportPhone { get; set; } = "+880 9610-123456";
        public string SupportEmail { get; set; } = "support@lamisamart.bd";
        public string BusinessHours { get; set; } = "Mon - Sat: 10AM - 9PM";
        public string StoreLocation { get; set; } = "Malibagh & Dhanmondi, Dhaka";
        public string CopyrightText { get; set; } = "© 2026 LamisaMart Bangladesh. All Rights Reserved.";
        public string FacebookUrl { get; set; } = "https://facebook.com/lamisamart.bd";
        public string InstagramUrl { get; set; } = "https://instagram.com/lamisamart.bd";
        public string YoutubeUrl { get; set; } = "https://youtube.com/@lamisamartbd";
        public string WhatsappNumber { get; set; } = "+8801700000000";
        public bool ShowBkash { get; set; } = true;
        public bool ShowNagad { get; set; } = true;
        public bool ShowVisa { get; set; } = true;
        public bool ShowMastercard { get; set; } = true;
        public bool ShowBanglaQR { get; set; } = true;
    }

    [BindProperty] public string FooterAboutText { get; set; } = string.Empty;
    [BindProperty] public string SupportPhone { get; set; } = string.Empty;
    [BindProperty] public string SupportEmail { get; set; } = string.Empty;
    [BindProperty] public string BusinessHours { get; set; } = string.Empty;
    [BindProperty] public string StoreLocation { get; set; } = string.Empty;
    [BindProperty] public string CopyrightText { get; set; } = string.Empty;
    [BindProperty] public string FacebookUrl { get; set; } = string.Empty;
    [BindProperty] public string InstagramUrl { get; set; } = string.Empty;
    [BindProperty] public string YoutubeUrl { get; set; } = string.Empty;
    [BindProperty] public string WhatsappNumber { get; set; } = string.Empty;
    [BindProperty] public bool ShowBkash { get; set; } = true;
    [BindProperty] public bool ShowNagad { get; set; } = true;
    [BindProperty] public bool ShowVisa { get; set; } = true;
    [BindProperty] public bool ShowMastercard { get; set; } = true;
    [BindProperty] public bool ShowBanglaQR { get; set; } = true;

    public void OnGet()
    {
        FooterAboutText = StoreSettings.FooterAboutText;
        SupportPhone = StoreSettings.SupportPhone;
        SupportEmail = StoreSettings.SupportEmail;
        BusinessHours = StoreSettings.BusinessHours;
        StoreLocation = StoreSettings.StoreLocation;
        CopyrightText = StoreSettings.CopyrightText;
        FacebookUrl = StoreSettings.FacebookUrl;
        InstagramUrl = StoreSettings.InstagramUrl;
        YoutubeUrl = StoreSettings.YoutubeUrl;
        WhatsappNumber = StoreSettings.WhatsappNumber;
        ShowBkash = StoreSettings.ShowBkash;
        ShowNagad = StoreSettings.ShowNagad;
        ShowVisa = StoreSettings.ShowVisa;
        ShowMastercard = StoreSettings.ShowMastercard;
        ShowBanglaQR = StoreSettings.ShowBanglaQR;
    }

    public IActionResult OnPost()
    {
        StoreSettings.FooterAboutText = FooterAboutText ?? string.Empty;
        StoreSettings.SupportPhone = SupportPhone ?? string.Empty;
        StoreSettings.SupportEmail = SupportEmail ?? string.Empty;
        StoreSettings.BusinessHours = BusinessHours ?? string.Empty;
        StoreSettings.StoreLocation = StoreLocation ?? string.Empty;
        StoreSettings.CopyrightText = CopyrightText ?? string.Empty;
        StoreSettings.FacebookUrl = FacebookUrl ?? string.Empty;
        StoreSettings.InstagramUrl = InstagramUrl ?? string.Empty;
        StoreSettings.YoutubeUrl = YoutubeUrl ?? string.Empty;
        StoreSettings.WhatsappNumber = WhatsappNumber ?? string.Empty;
        StoreSettings.ShowBkash = ShowBkash;
        StoreSettings.ShowNagad = ShowNagad;
        StoreSettings.ShowVisa = ShowVisa;
        StoreSettings.ShowMastercard = ShowMastercard;
        StoreSettings.ShowBanglaQR = ShowBanglaQR;

        TempData["SuccessMessage"] = "Storefront Footer UI Customizations saved and published live!";
        return RedirectToPage();
    }
}
