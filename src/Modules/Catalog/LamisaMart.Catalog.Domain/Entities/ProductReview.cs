using LamisaMart.Shared.Domain;

namespace LamisaMart.Catalog.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; } // 1 to 5 stars
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; } = true;
    public bool IsApproved { get; set; } = true;

    public Product Product { get; set; } = null!;
}
