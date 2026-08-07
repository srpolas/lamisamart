using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using LamisaMart.Payments.Application.Common.Interfaces;
using LamisaMart.Payments.Domain.Entities;
using LamisaMart.Payments.Infrastructure.Settings;

namespace LamisaMart.Payments.Infrastructure.Services;

public class SSLCommerzClient : ISSLCommerzClient
{
    private readonly HttpClient _httpClient;
    private readonly SSLCommerzSettings _settings;

    public SSLCommerzClient(HttpClient httpClient, IOptions<SSLCommerzSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<InitiatePaymentResult> InitiatePaymentAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        var postData = new Dictionary<string, string>
        {
            { "store_id", _settings.StoreId },
            { "store_passwd", _settings.StorePassword },
            { "total_amount", transaction.Amount.Amount.ToString("0.00") },
            { "currency", transaction.Amount.Currency },
            { "tran_id", transaction.TransactionId },
            { "success_url", _settings.SuccessUrl },
            { "fail_url", _settings.FailUrl },
            { "cancel_url", _settings.CancelUrl },
            { "ipn_url", _settings.IpnUrl },
            { "cus_name", transaction.CustomerName },
            { "cus_email", transaction.CustomerEmail },
            { "cus_phone", transaction.CustomerPhone },
            { "cus_add1", "Dhaka" },
            { "cus_city", "Dhaka" },
            { "cus_country", "Bangladesh" },
            { "shipping_method", "NO" },
            { "product_name", "LamisaMart Order" },
            { "product_category", "Ecommerce" },
            { "product_profile", "general" }
        };

        var content = new FormUrlEncodedContent(postData);
        var url = $"{_settings.BaseUrl}/gwprocess/v4/api.php";
        
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        
        transaction.InitiationResponsePayload = jsonString;

        if (response.IsSuccessStatusCode)
        {
            // Simple parsing to avoid complex JSON objects in this stub
            // Expected response contains GatewayPageURL and sessionkey
            // (In a real implementation we would deserialize to a strongly typed class)
            if (jsonString.Contains("GatewayPageURL") && jsonString.Contains("status\":\"SUCCESS"))
            {
                // Simplified extraction for the sake of this phase
                // E.g., {"status":"SUCCESS","sessionkey":"XYZ","GatewayPageURL":"https://sandbox.sslcommerz.com/gwprocess/v4/gw.php?Q=pay&SESSIONKEY=XYZ"}
                
                // For now, let's just create a basic dummy extraction or use regex
                // But typically we use System.Text.Json.JsonDocument
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement;
                if (root.TryGetProperty("status", out var statusEl) && statusEl.GetString() == "SUCCESS")
                {
                    var gatewayUrl = root.GetProperty("GatewayPageURL").GetString();
                    var sessionKey = root.GetProperty("sessionkey").GetString();
                    
                    if (gatewayUrl != null && sessionKey != null)
                    {
                        return new InitiatePaymentResult(true, gatewayUrl, sessionKey);
                    }
                }
            }
        }

        return new InitiatePaymentResult(false, "", "", "Failed to initiate payment with SSLCommerz.");
    }

    public async Task<ValidatePaymentResult> ValidatePaymentAsync(string valId, CancellationToken cancellationToken = default)
    {
        var url = $"{_settings.BaseUrl}/validator/api/validationserverAPI.php?val_id={valId}&store_id={_settings.StoreId}&store_passwd={_settings.StorePassword}&v=1&format=json";
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;
            
            if (root.TryGetProperty("status", out var statusEl))
            {
                var status = statusEl.GetString();
                if (status == "VALID" || status == "VALIDATED")
                {
                    var amount = root.GetProperty("amount").GetString() ?? "0";
                    return new ValidatePaymentResult(true, amount);
                }
            }
        }
        
        return new ValidatePaymentResult(false, "0", "Payment validation failed.");
    }
}
