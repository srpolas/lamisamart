using LamisaMart.Shared.Domain;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Catalog.Domain.Entities;

public class Product : BaseEntity
{
    public Guid VendorId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public Money BasePrice { get; set; } = new(0m);
    public Money CompareAtPrice { get; set; } = new(0m);
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public double AverageRating { get; set; } = 0.0;
    public int ReviewCount { get; set; } = 0;
    public int TotalSales { get; set; } = 0;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}
