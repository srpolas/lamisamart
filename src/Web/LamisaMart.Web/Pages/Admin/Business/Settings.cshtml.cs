using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Business;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class SettingsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string ActiveTab { get; set; } = "general";

    // 1st Tab: General Business Info
    [BindProperty] public string StoreName { get; set; } = "LamisaMart Bangladesh";
    [BindProperty] public string LegalEntityName { get; set; } = "Lamisa Trading Co. Ltd.";
    [BindProperty] public string TradeLicenseNo { get; set; } = "TRAD/DNCC/029104/2025";
    [BindProperty] public string BinVatNumber { get; set; } = "004819204-0101";
    [BindProperty] public string SupportEmail { get; set; } = "support@lamisamart.bd";
    [BindProperty] public string SupportPhone { get; set; } = "+880 9610-123456";
    [BindProperty] public string StoreAddress { get; set; } = "Level 5, Fortune Shopping Complex, Malibagh, Dhaka-1217";
    [BindProperty] public string Currency { get; set; } = "BDT (৳)";
    [BindProperty] public string Timezone { get; set; } = "(GMT+06:00) Asia/Dhaka";

    // 2nd Tab: Products
    [BindProperty] public int LowStockThreshold { get; set; } = 5;
    [BindProperty] public bool EnableProductReviews { get; set; } = true;
    [BindProperty] public bool AllowOutofStockPurchases { get; set; } = false;
    [BindProperty] public string WeightUnit { get; set; } = "kg";
    [BindProperty] public string DimensionUnit { get; set; } = "cm";

    // 3rd Tab: Shipping
    [BindProperty] public decimal InsideDhakaFee { get; set; } = 60m;
    [BindProperty] public decimal OutsideDhakaFee { get; set; } = 120m;
    [BindProperty] public decimal FreeShippingThreshold { get; set; } = 5000m;
    [BindProperty] public string DefaultCourierPartner { get; set; } = "Pathao Courier Express";

    // 4th Tab: Payment Setup
    [BindProperty] public bool EnableBkashPayment { get; set; } = true;
    [BindProperty] public string BkashMerchantNumber { get; set; } = "01700000000";
    [BindProperty] public bool EnableNagadPayment { get; set; } = true;
    [BindProperty] public string NagadMerchantNumber { get; set; } = "01800000000";
    [BindProperty] public bool EnableCashOnDelivery { get; set; } = true;
    [BindProperty] public bool EnableSSLCommerz { get; set; } = true;
    [BindProperty] public string SSLCommerzStoreId { get; set; } = "lamisamart_live";
    [BindProperty] public bool SSLCommerzSandbox { get; set; } = false;

    // 5th Tab: Accounts & Privacy
    [BindProperty] public bool AllowGuestCheckout { get; set; } = true;
    [BindProperty] public bool EnableRegistration { get; set; } = true;
    [BindProperty] public int DataRetentionDays { get; set; } = 365;
    [BindProperty] public string PrivacyPolicyUrl { get; set; } = "/privacy";
    [BindProperty] public string TermsOfServiceUrl { get; set; } = "/terms";

    // 6th Tab: Email Notification
    [BindProperty] public string SmtpHost { get; set; } = "smtp.mailgun.org";
    [BindProperty] public int SmtpPort { get; set; } = 587;
    [BindProperty] public string SmtpUsername { get; set; } = "postmaster@lamisamart.bd";
    [BindProperty] public string SmtpPassword { get; set; } = "••••••••••••";
    [BindProperty] public bool EnableSsl { get; set; } = true;
    [BindProperty] public string AdminNotificationEmail { get; set; } = "admin@lamisamart.bd";
    [BindProperty] public bool NotifyCustomerOrderPlaced { get; set; } = true;
    [BindProperty] public bool NotifyCustomerOrderShipped { get; set; } = true;
    [BindProperty] public bool NotifyCustomerOrderDelivered { get; set; } = true;

    // 7th Tab: Shop Visibility
    [BindProperty] public bool MaintenanceMode { get; set; } = false;
    [BindProperty] public string AnnouncementBannerText { get; set; } = "🎉 Exclusive Jamdani & Katan Collection Live! Enjoy Free Shipping on orders over ৳5,000!";
    [BindProperty] public bool PasswordProtectStore { get; set; } = false;
    [BindProperty] public bool HidePricesForGuests { get; set; } = false;

    // 8th Tab: Advanced
    [BindProperty] public string GoogleAnalyticsId { get; set; } = "G-LM8901234";
    [BindProperty] public string FacebookPixelId { get; set; } = "8920194820194";
    [BindProperty] public string WebhookEndpoint { get; set; } = "https://lamisamart.elink.bd/api/webhooks/orders";
    [BindProperty] public bool EnableRestApi { get; set; } = true;

    public void OnGet()
    {
    }

    public IActionResult OnPost(string tabName)
    {
        var targetTab = !string.IsNullOrWhiteSpace(tabName) ? tabName : ActiveTab;
        TempData["SuccessMessage"] = $"Settings for '{GetTabDisplayName(targetTab)}' updated successfully!";
        return RedirectToPage(new { activeTab = targetTab });
    }

    private static string GetTabDisplayName(string tab)
    {
        return tab.ToLower() switch
        {
            "general" => "General Business Info",
            "products" => "Products Settings",
            "shipping" => "Shipping Rates",
            "payment" => "Payment Setup",
            "privacy" => "Accounts & Privacy",
            "email" => "Email Notification",
            "visibility" => "Shop Visibility",
            "advanced" => "Advanced Configurations",
            _ => "Business Settings"
        };
    }
}
