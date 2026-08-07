using Microsoft.EntityFrameworkCore;
using LamisaMart.PageBuilder.Domain.Entities;

namespace LamisaMart.PageBuilder.Application.Common.Interfaces;

public interface IPageBuilderDbContext
{
    DbSet<PageLayout> PageLayouts { get; }
    DbSet<PageSection> PageSections { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
