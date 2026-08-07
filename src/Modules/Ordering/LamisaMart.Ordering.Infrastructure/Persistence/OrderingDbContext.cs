using Microsoft.EntityFrameworkCore;
using LamisaMart.Ordering.Domain.Entities;

using LamisaMart.Ordering.Application.Common.Interfaces;

namespace LamisaMart.Ordering.Infrastructure.Persistence;

public class OrderingDbContext : DbContext, IOrderingDbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options) { }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<VendorSubOrder> VendorSubOrders => Set<VendorSubOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("ordering");

        // Cart Entity Configuration
        modelBuilder.Entity<Cart>(builder =>
        {
            builder.ToTable("Carts");
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.SessionId);
            builder.HasIndex(c => c.CustomerId);
        });

        // CartItem Entity Configuration
        modelBuilder.Entity<CartItem>(builder =>
        {
            builder.ToTable("CartItems");
            builder.HasKey(ci => ci.Id);
            builder.Property(ci => ci.UnitPrice).HasPrecision(18, 2);

            builder.HasOne(ci => ci.Cart)
                   .WithMany(c => c.Items)
                   .HasForeignKey(ci => ci.CartId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // Order Entity Configuration
        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(o => o.CustomerId);

            builder.ComplexProperty(o => o.TotalAmount, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("TotalCurrency").HasMaxLength(3);
            });

            builder.ComplexProperty(o => o.ShippingFee, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("ShippingFeeAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("ShippingFeeCurrency").HasMaxLength(3);
            });

            builder.ComplexProperty(o => o.ShippingAddress, addr =>
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
        });

        // VendorSubOrder Entity Configuration
        modelBuilder.Entity<VendorSubOrder>(builder =>
        {
            builder.ToTable("VendorSubOrders");
            builder.HasKey(so => so.Id);
            builder.Property(so => so.SubOrderNumber).HasMaxLength(50).IsRequired();
            builder.HasIndex(so => so.SubOrderNumber).IsUnique();
            builder.HasIndex(so => so.VendorId);

            builder.ComplexProperty(so => so.SubTotal, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("SubTotalAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("SubTotalCurrency").HasMaxLength(3);
            });

            builder.ComplexProperty(so => so.ShippingFee, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("ShippingFeeAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("ShippingFeeCurrency").HasMaxLength(3);
            });

            builder.ComplexProperty(so => so.CommissionAmount, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("CommissionAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("CommissionCurrency").HasMaxLength(3);
            });

            builder.ComplexProperty(so => so.VendorPayoutAmount, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("VendorPayoutAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("VendorPayoutCurrency").HasMaxLength(3);
            });

            builder.HasOne(so => so.Order)
                   .WithMany(o => o.VendorSubOrders)
                   .HasForeignKey(so => so.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem Entity Configuration
        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.Id);

            builder.ComplexProperty(oi => oi.UnitPrice, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });

            builder.HasOne(oi => oi.VendorSubOrder)
                   .WithMany(so => so.Items)
                   .HasForeignKey(oi => oi.VendorSubOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
