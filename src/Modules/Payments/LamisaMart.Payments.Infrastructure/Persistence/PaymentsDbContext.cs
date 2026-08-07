using Microsoft.EntityFrameworkCore;
using LamisaMart.Payments.Domain.Entities;
using LamisaMart.Payments.Application.Common.Interfaces;

namespace LamisaMart.Payments.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext, IPaymentsDbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("payments");

        modelBuilder.Entity<PaymentTransaction>(builder =>
        {
            builder.ToTable("PaymentTransactions");
            builder.HasKey(pt => pt.Id);
            builder.Property(pt => pt.TransactionId).HasMaxLength(50).IsRequired();
            builder.HasIndex(pt => pt.TransactionId).IsUnique();
            builder.HasIndex(pt => pt.OrderNumber);
            builder.HasIndex(pt => pt.SessionKey);

            builder.ComplexProperty(pt => pt.Amount, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount).HasColumnName("TransactionAmount").HasPrecision(18, 2);
                priceBuilder.Property(m => m.Currency).HasColumnName("TransactionCurrency").HasMaxLength(3);
            });
        });
    }
}
