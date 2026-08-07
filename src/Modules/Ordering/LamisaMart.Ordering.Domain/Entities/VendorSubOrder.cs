using LamisaMart.Shared.Domain;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Ordering.Domain.Entities;

public enum SubOrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Returned
}

public class VendorSubOrder : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid VendorId { get; set; }
    public string SubOrderNumber { get; set; } = string.Empty; // SUB-YYYYMMDD-XXXXX-V1
    public string VendorName { get; set; } = string.Empty;
    
    public Money SubTotal { get; set; } = new(0m);
    public Money ShippingFee { get; set; } = new(0m);
    public Money CommissionAmount { get; set; } = new(0m);
    public Money VendorPayoutAmount { get; set; } = new(0m);
    
    public SubOrderStatus Status { get; set; } = SubOrderStatus.Pending;
    public string? CourierName { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public Order Order { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
