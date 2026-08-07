using LamisaMart.Shared.Domain;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Catalog.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public Money Price { get; set; } = new(0m);
    public Money CompareAtPrice { get; set; } = new(0m);
    public int StockQuantity { get; set; }
    public string AttributesJson { get; set; } = "{}";
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Product Product { get; set; } = null!;
}
