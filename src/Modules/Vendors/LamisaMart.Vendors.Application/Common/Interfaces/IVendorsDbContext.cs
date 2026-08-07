using Microsoft.EntityFrameworkCore;
using LamisaMart.Vendors.Domain.Entities;

namespace LamisaMart.Vendors.Application.Common.Interfaces;

public interface IVendorsDbContext
{
    DbSet<Vendor> Vendors { get; }
    DbSet<ShopProfile> ShopProfiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
