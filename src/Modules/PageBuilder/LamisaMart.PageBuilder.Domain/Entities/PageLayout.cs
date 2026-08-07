using LamisaMart.Shared.Domain;

namespace LamisaMart.PageBuilder.Domain.Entities;

public enum PageType
{
    Home,
    VendorShop,
    Category,
    Custom
}

public class PageLayout : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Main Home Page v1"
    public string Route { get; set; } = string.Empty; // e.g., "/", "/shop/vintage-boutique"
    public PageType Type { get; set; } = PageType.Custom;
    
    // If it belongs to a specific vendor shop (null for platform pages like Home)
    public Guid? VendorId { get; set; } 
    
    public bool IsActive { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
    
    // Global page settings (e.g., body background color, font overrides) stored as JSON
    public string SettingsJson { get; set; } = "{}";

    public ICollection<PageSection> Sections { get; set; } = new List<PageSection>();
}
