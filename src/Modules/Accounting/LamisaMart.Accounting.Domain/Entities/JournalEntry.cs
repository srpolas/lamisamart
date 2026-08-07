using LamisaMart.Shared.Domain;

namespace LamisaMart.Accounting.Domain.Entities;

public enum JournalEntryStatus
{
    Draft,
    Posted,
    Voided
}

public class JournalEntry : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty; // E.g., ORD-YYYYMMDD-XXXXX-V1
    public string Description { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Posted;
    
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
