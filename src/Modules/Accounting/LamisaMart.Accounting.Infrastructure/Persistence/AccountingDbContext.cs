using Microsoft.EntityFrameworkCore;
using LamisaMart.Accounting.Domain.Entities;
using LamisaMart.Accounting.Application.Common.Interfaces;

namespace LamisaMart.Accounting.Infrastructure.Persistence;

public class AccountingDbContext : DbContext, IAccountingDbContext
{
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options) { }

    public DbSet<LedgerAccount> LedgerAccounts => Set<LedgerAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("accounting");

        modelBuilder.Entity<LedgerAccount>(builder =>
        {
            builder.ToTable("LedgerAccounts");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.AccountCode).HasMaxLength(50).IsRequired();
            builder.HasIndex(a => a.AccountCode).IsUnique();
            builder.HasIndex(a => a.VendorId);
            builder.Property(a => a.CurrentBalance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<JournalEntry>(builder =>
        {
            builder.ToTable("JournalEntries");
            builder.HasKey(je => je.Id);
            builder.Property(je => je.ReferenceNumber).HasMaxLength(100);
            builder.HasIndex(je => je.ReferenceNumber);
        });

        modelBuilder.Entity<JournalEntryLine>(builder =>
        {
            builder.ToTable("JournalEntryLines");
            builder.HasKey(jel => jel.Id);
            builder.Property(jel => jel.Debit).HasPrecision(18, 2);
            builder.Property(jel => jel.Credit).HasPrecision(18, 2);

            builder.HasOne(jel => jel.JournalEntry)
                   .WithMany(je => je.Lines)
                   .HasForeignKey(jel => jel.JournalEntryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(jel => jel.LedgerAccount)
                   .WithMany()
                   .HasForeignKey(jel => jel.LedgerAccountId)
                   .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
