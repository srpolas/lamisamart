using LamisaMart.Shared.Domain;

namespace LamisaMart.Accounting.Domain.Entities;

public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; set; }
    public Guid LedgerAccountId { get; set; }
    
    public decimal Debit { get; set; } = 0m;
    public decimal Credit { get; set; } = 0m;

    public JournalEntry JournalEntry { get; set; } = null!;
    public LedgerAccount LedgerAccount { get; set; } = null!;
}
