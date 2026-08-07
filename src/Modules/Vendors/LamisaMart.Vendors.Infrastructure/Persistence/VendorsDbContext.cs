using Microsoft.EntityFrameworkCore;
using LamisaMart.Vendors.Domain.Entities;
using LamisaMart.Vendors.Application.Common.Interfaces;

namespace LamisaMart.Vendors.Infrastructure.Persistence;

public class VendorsDbContext : DbContext, IVendorsDbContext
{
    public VendorsDbContext(DbContextOptions<VendorsDbContext> options) : base(options) { }

    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<ShopProfile> ShopProfiles => Set<ShopProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("vendors");

        modelBuilder.Entity<Vendor>(builder =>
        {
            builder.ToTable("Vendors");
            builder.HasKey(v => v.Id);
            builder.HasIndex(v => v.OwnerId);

            builder.ComplexProperty(v => v.BusinessAddress, addr =>
            {
                addr.Property(a => a.RecipientName).HasMaxLength(150);
                addr.Property(a => a.PhoneNumber).HasMaxLength(20);
                addr.Property(a => a.StreetAddress).HasMaxLength(300);
                addr.Property(a => a.ThanaUpazila).HasMaxLength(100);
                addr.Property(a => a.District).HasMaxLength(100);
                addr.Property(a => a.Division).HasMaxLength(100);
                addr.Property(a => a.PostalCode).HasMaxLength(20);
                addr.Property(a => a.Country).HasMaxLength(50);
            });
            
            builder.HasOne(v => v.Profile)
                   .WithOne(p => p.Vendor)
                   .HasForeignKey<ShopProfile>(p => p.VendorId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShopProfile>(builder =>
        {
            builder.ToTable("ShopProfiles");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Slug).HasMaxLength(150).IsRequired();
            builder.HasIndex(p => p.Slug).IsUnique();
        });
    }
}
