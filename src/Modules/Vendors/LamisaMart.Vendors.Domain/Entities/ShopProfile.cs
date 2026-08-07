using LamisaMart.Shared.Domain;

namespace LamisaMart.Vendors.Domain.Entities;

public class ShopProfile : BaseEntity
{
    public Guid VendorId { get; set; }
    
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // Store URL slug
    public string Description { get; set; } = string.Empty;
    
    public string LogoUrl { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
    
    public decimal Rating { get; set; } = 0m;
    public int TotalReviews { get; set; } = 0;

    public Vendor Vendor { get; set; } = null!;
}
