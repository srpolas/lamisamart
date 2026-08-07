using Microsoft.EntityFrameworkCore;
using LamisaMart.Accounting.Domain.Entities;

namespace LamisaMart.Accounting.Application.Common.Interfaces;

public interface IAccountingDbContext
{
    DbSet<LedgerAccount> LedgerAccounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryLine> JournalEntryLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
