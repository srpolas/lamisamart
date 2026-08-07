using LamisaMart.Shared.Domain;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Payments.Domain.Entities;

public enum PaymentStatus
{
    Pending,
    Processing,
    Success,
    Failed,
    Cancelled,
    Refunded
}

public class PaymentTransaction : BaseEntity
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty; // Unique ID sent to gateway
    
    public string GatewayName { get; set; } = string.Empty; // "SSLCommerz", "BanglaQR"
    public string GatewayTransactionId { get; set; } = string.Empty; // Bank/Gateway's transaction ID
    public string SessionKey { get; set; } = string.Empty; // For SSLCommerz
    
    public Money Amount { get; set; } = new(0m);
    
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Stores raw JSON responses for audit/debugging
    public string InitiationResponsePayload { get; set; } = string.Empty;
    public string ValidationResponsePayload { get; set; } = string.Empty;
    
    public DateTime? PaidAt { get; set; }
}
