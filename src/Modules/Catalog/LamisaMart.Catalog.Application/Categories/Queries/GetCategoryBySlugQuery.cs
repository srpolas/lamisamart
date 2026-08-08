using MediatR;
using Microsoft.EntityFrameworkCore;
using LamisaMart.Catalog.Application.Common.Interfaces;
using LamisaMart.Catalog.Application.DTOs;

namespace LamisaMart.Catalog.Application.Categories.Queries;

public record GetCategoryBySlugQuery(string Slug) : IRequest<CategoryDto?>;

public class GetCategoryBySlugQueryHandler : IRequestHandler<GetCategoryBySlugQuery, CategoryDto?>
{
    private readonly ICatalogDbContext _context;

    public GetCategoryBySlugQueryHandler(ICatalogDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto?> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Slug))
            return null;

        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Slug.ToLower() == request.Slug.ToLower() && c.IsActive && !c.IsDeleted, cancellationToken);

        if (category == null)
            return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ImageUrl = category.ImageUrl,
            ParentCategoryId = category.ParentCategoryId,
            DisplayOrder = category.DisplayOrder,
            IsFeatured = category.IsFeatured,
            SubCategories = category.SubCategories
                .Where(s => s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new CategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    ImageUrl = s.ImageUrl,
                    ParentCategoryId = s.ParentCategoryId,
                    DisplayOrder = s.DisplayOrder,
                    IsFeatured = s.IsFeatured
                }).ToList()
        };
    }
}
