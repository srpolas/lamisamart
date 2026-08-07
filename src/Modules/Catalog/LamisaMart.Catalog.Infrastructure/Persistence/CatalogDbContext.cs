using Microsoft.EntityFrameworkCore;
using LamisaMart.Catalog.Domain.Entities;

using LamisaMart.Catalog.Application.Common.Interfaces;

namespace LamisaMart.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext, ICatalogDbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("catalog");

        // Category Entity Configuration
        modelBuilder.Entity<Category>(builder =>
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
            builder.Property(c => c.Slug).HasMaxLength(150).IsRequired();
            builder.HasIndex(c => c.Slug).IsUnique();

            builder.HasOne(c => c.ParentCategory)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(c => c.ParentCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        // Product Entity Configuration
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
            builder.Property(p => p.Slug).HasMaxLength(250).IsRequired();
            builder.HasIndex(p => p.Slug).IsUnique();
            builder.HasIndex(p => p.VendorId);
            builder.HasIndex(p => p.CategoryId);

            builder.ComplexProperty(p => p.BasePrice, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("BasePriceAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("BasePriceCurrency").HasMaxLength(3);
            });

            builder.ComplexProperty(p => p.CompareAtPrice, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("CompareAtPriceAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("CompareAtPriceCurrency").HasMaxLength(3);
            });

            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductVariant Entity Configuration
        modelBuilder.Entity<ProductVariant>(builder =>
        {
            builder.ToTable("ProductVariants");
            builder.HasKey(pv => pv.Id);
            builder.Property(pv => pv.SKU).HasMaxLength(100).IsRequired();
            builder.HasIndex(pv => pv.SKU).IsUnique();
            builder.Property(pv => pv.RowVersion).IsRowVersion();

            builder.ComplexProperty(pv => pv.Price, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
            });

            builder.ComplexProperty(pv => pv.CompareAtPrice, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("CompareAtPriceAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("CompareAtPriceCurrency").HasMaxLength(3);
            });

            builder.HasOne(pv => pv.Product)
                   .WithMany(p => p.Variants)
                   .HasForeignKey(pv => pv.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductImage Entity Configuration
        modelBuilder.Entity<ProductImage>(builder =>
        {
            builder.ToTable("ProductImages");
            builder.HasKey(pi => pi.Id);
            builder.Property(pi => pi.ImageUrl).HasMaxLength(500).IsRequired();

            builder.HasOne(pi => pi.Product)
                   .WithMany(p => p.Images)
                   .HasForeignKey(pi => pi.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductReview Entity Configuration
        modelBuilder.Entity<ProductReview>(builder =>
        {
            builder.ToTable("ProductReviews");
            builder.HasKey(pr => pr.Id);
            builder.Property(pr => pr.Comment).HasMaxLength(2000);

            builder.HasOne(pr => pr.Product)
                   .WithMany(p => p.Reviews)
                   .HasForeignKey(pr => pr.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
