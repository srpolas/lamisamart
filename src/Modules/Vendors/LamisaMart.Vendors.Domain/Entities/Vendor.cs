using LamisaMart.Shared.Domain;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Vendors.Domain.Entities;

public enum VendorStatus
{
    Pending,
    Active,
    Suspended,
    Closed
}

public class Vendor : BaseEntity
{
    public Guid OwnerId { get; set; } // Links to Identity User
    public string BusinessName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    
    public VendorStatus Status { get; set; } = VendorStatus.Pending;
    public Address BusinessAddress { get; set; } = new();

    public ShopProfile Profile { get; set; } = null!;
}
