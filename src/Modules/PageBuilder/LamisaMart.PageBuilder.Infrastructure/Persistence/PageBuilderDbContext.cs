using Microsoft.EntityFrameworkCore;
using LamisaMart.PageBuilder.Domain.Entities;
using LamisaMart.PageBuilder.Application.Common.Interfaces;

namespace LamisaMart.PageBuilder.Infrastructure.Persistence;

public class PageBuilderDbContext : DbContext, IPageBuilderDbContext
{
    public PageBuilderDbContext(DbContextOptions<PageBuilderDbContext> options) : base(options) { }

    public DbSet<PageLayout> PageLayouts => Set<PageLayout>();
    public DbSet<PageSection> PageSections => Set<PageSection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("cms");

        modelBuilder.Entity<PageLayout>(builder =>
        {
            builder.ToTable("PageLayouts");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Route).HasMaxLength(255).IsRequired();
            builder.HasIndex(p => p.Route); // Not unique because multiple layouts can exist for one route (e.g., drafts vs active)
            builder.HasIndex(p => p.VendorId);
        });

        modelBuilder.Entity<PageSection>(builder =>
        {
            builder.ToTable("PageSections");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.SectionType).HasMaxLength(100).IsRequired();

            builder.HasOne(s => s.PageLayout)
                   .WithMany(p => p.Sections)
                   .HasForeignKey(s => s.PageLayoutId)
                   .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
