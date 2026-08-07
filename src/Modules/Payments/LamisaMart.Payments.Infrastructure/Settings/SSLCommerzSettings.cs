namespace LamisaMart.Payments.Infrastructure.Settings;

public class SSLCommerzSettings
{
    public const string SectionName = "SSLCommerz";
    public string StoreId { get; set; } = string.Empty;
    public string StorePassword { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
    public string BaseUrl => IsSandbox ? "https://sandbox.sslcommerz.com" : "https://securepay.sslcommerz.com";
    
    // Callback URLs
    public string SuccessUrl { get; set; } = string.Empty;
    public string FailUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty; // Instant Payment Notification
}
