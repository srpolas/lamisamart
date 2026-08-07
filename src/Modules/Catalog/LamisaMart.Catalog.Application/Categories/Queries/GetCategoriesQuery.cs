using MediatR;
using Microsoft.EntityFrameworkCore;
using LamisaMart.Catalog.Application.Common.Interfaces;
using LamisaMart.Catalog.Application.DTOs;

namespace LamisaMart.Catalog.Application.Categories.Queries;

public record GetCategoriesQuery(bool OnlyActive = true) : IRequest<List<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICatalogDbContext _context;

    public GetCategoriesQueryHandler(ICatalogDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categories.AsNoTracking();

        if (request.OnlyActive)
        {
            query = query.Where(c => c.IsActive && !c.IsDeleted);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                DisplayOrder = c.DisplayOrder,
                IsFeatured = c.IsFeatured
            })
            .ToListAsync(cancellationToken);

        return categories;
    }
}
