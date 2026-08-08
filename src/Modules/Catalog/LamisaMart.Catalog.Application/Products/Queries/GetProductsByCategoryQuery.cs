using MediatR;
using Microsoft.EntityFrameworkCore;
using LamisaMart.Catalog.Application.Common.Interfaces;
using LamisaMart.Catalog.Application.DTOs;

namespace LamisaMart.Catalog.Application.Products.Queries;

public record GetProductsByCategoryQuery(
    string CategorySlug, 
    string? SortBy = null, 
    decimal? MinPrice = null, 
    decimal? MaxPrice = null,
    string? SubCategorySlug = null
) : IRequest<List<ProductDto>>;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, List<ProductDto>>
{
    private readonly ICatalogDbContext _context;

    public GetProductsByCategoryQueryHandler(ICatalogDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var categorySlug = request.CategorySlug.ToLower().Trim();

        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug.ToLower() == categorySlug && c.IsActive && !c.IsDeleted, cancellationToken);

        if (category == null && categorySlug != "all" && categorySlug != "new-arrival" && categorySlug != "sale")
        {
            return new List<ProductDto>();
        }

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsPublished && !p.IsDeleted);

        if (categorySlug == "new-arrival")
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }
        else if (categorySlug == "sale")
        {
            query = query.Where(p => p.CompareAtPrice.Amount > p.BasePrice.Amount);
        }
        else if (category != null)
        {
            var categoryIds = new List<Guid> { category.Id };
            var subCategoryIds = await _context.Categories
                .Where(c => c.ParentCategoryId == category.Id && c.IsActive && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            categoryIds.AddRange(subCategoryIds);

            if (!string.IsNullOrWhiteSpace(request.SubCategorySlug))
            {
                var targetSubCat = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Slug.ToLower() == request.SubCategorySlug.ToLower(), cancellationToken);
                if (targetSubCat != null)
                {
                    categoryIds = new List<Guid> { targetSubCat.Id };
                }
            }

            query = query.Where(p => categoryIds.Contains(p.CategoryId));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice.Amount >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice.Amount <= request.MaxPrice.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "price-low" => query.OrderBy(p => p.BasePrice.Amount),
            "price-high" => query.OrderByDescending(p => p.BasePrice.Amount),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.TotalSales)
        };

        var products = await query
            .Select(p => new ProductDto
            {
                Id = p.Id,
                VendorId = p.VendorId,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                BasePriceAmount = p.BasePrice.Amount,
                Currency = p.BasePrice.Currency,
                CompareAtPriceAmount = p.CompareAtPrice.Amount,
                PrimaryImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault() ?? string.Empty,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                IsFeatured = p.IsFeatured
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}
