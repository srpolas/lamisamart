namespace LamisaMart.Catalog.Application.DTOs;

public record ProductDto
{
    public Guid Id { get; init; }
    public Guid VendorId { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public decimal BasePriceAmount { get; init; }
    public string Currency { get; init; } = "BDT";
    public decimal CompareAtPriceAmount { get; init; }
    public string PrimaryImageUrl { get; init; } = string.Empty;
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
    public bool IsFeatured { get; init; }
}
