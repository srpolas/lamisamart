using Microsoft.EntityFrameworkCore;
using LamisaMart.Catalog.Domain.Entities;

namespace LamisaMart.Catalog.Application.Common.Interfaces;

public interface ICatalogDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductReview> ProductReviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
