using MediatR;
using Microsoft.EntityFrameworkCore;
using LamisaMart.PageBuilder.Application.Common.Interfaces;

namespace LamisaMart.PageBuilder.Application.Layouts.Queries;

public record PageLayoutDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string SettingsJson { get; init; } = "{}";
    public List<PageSectionDto> Sections { get; init; } = new();
}

public record PageSectionDto
{
    public Guid Id { get; init; }
    public string SectionType { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public string ContentPayloadJson { get; init; } = "{}";
}

public record GetPageLayoutQuery(string Route, Guid? VendorId = null) : IRequest<PageLayoutDto?>;

public class GetPageLayoutQueryHandler : IRequestHandler<GetPageLayoutQuery, PageLayoutDto?>
{
    private readonly IPageBuilderDbContext _context;

    public GetPageLayoutQueryHandler(IPageBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<PageLayoutDto?> Handle(GetPageLayoutQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PageLayouts
            .Include(pl => pl.Sections.Where(s => s.IsVisible).OrderBy(s => s.SortOrder))
            .Where(pl => pl.Route == request.Route && pl.IsActive);

        if (request.VendorId.HasValue)
        {
            query = query.Where(pl => pl.VendorId == request.VendorId);
        }
        else
        {
            query = query.Where(pl => pl.VendorId == null); // Platform pages
        }

        var layout = await query.FirstOrDefaultAsync(cancellationToken);

        if (layout == null) return null;

        return new PageLayoutDto
        {
            Id = layout.Id,
            Name = layout.Name,
            Route = layout.Route,
            Type = layout.Type.ToString(),
            SettingsJson = layout.SettingsJson,
            Sections = layout.Sections.Select(s => new PageSectionDto
            {
                Id = s.Id,
                SectionType = s.SectionType,
                SortOrder = s.SortOrder,
                ContentPayloadJson = s.ContentPayloadJson
            }).ToList()
        };
    }
}
