using LamisaMart.Shared.Domain;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Ordering.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid VendorSubOrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductVariantId { get; set; }
    
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    
    public Money UnitPrice { get; set; } = new(0m);
    public int Quantity { get; set; }
    public Money LineTotal => UnitPrice * Quantity;

    public VendorSubOrder VendorSubOrder { get; set; } = null!;
}
