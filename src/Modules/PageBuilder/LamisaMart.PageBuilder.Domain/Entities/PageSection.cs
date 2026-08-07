using LamisaMart.Shared.Domain;

namespace LamisaMart.PageBuilder.Domain.Entities;

public class PageSection : BaseEntity
{
    public Guid PageLayoutId { get; set; }
    
    // Identifier for the Razor Partial to render (e.g., "HeroBanner", "ProductCarousel", "CategoryGrid", "TrustBadges")
    public string SectionType { get; set; } = string.Empty; 
    
    // Display order in the page
    public int SortOrder { get; set; } = 0;
    
    // The configuration payload for this specific section (JSON format)
    // E.g., for HeroBanner: { "imageUrl": "/img/banner.jpg", "title": "Summer Sale", "buttonText": "Shop Now" }
    public string ContentPayloadJson { get; set; } = "{}";
    
    // Allows toggling a section off without deleting it
    public bool IsVisible { get; set; } = true;

    public PageLayout PageLayout { get; set; } = null!;
}
