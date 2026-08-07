using LamisaMart.Shared.Domain;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Ordering.Domain.Entities;

public enum OrderStatus
{
    Pending,
    PaymentProcessing,
    Confirmed,
    Processing,
    PartiallyShipped,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // ORD-YYYYMMDD-XXXXX
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    
    public Address ShippingAddress { get; set; } = new();
    public Money TotalAmount { get; set; } = new(0m);
    public Money ShippingFee { get; set; } = new(0m);
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string PaymentMethod { get; set; } = "SSLCommerz";
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }

    public ICollection<VendorSubOrder> VendorSubOrders { get; set; } = new List<VendorSubOrder>();
}
