using LamisaMart.Shared.Domain;

namespace LamisaMart.Ordering.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid? CustomerId { get; set; } // Null for anonymous session carts
    public string SessionId { get; set; } = string.Empty;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid VendorId { get; set; }
    
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public Cart Cart { get; set; } = null!;
}
