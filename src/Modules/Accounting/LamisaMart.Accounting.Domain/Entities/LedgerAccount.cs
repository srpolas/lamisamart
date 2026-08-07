using LamisaMart.Shared.Domain;

namespace LamisaMart.Accounting.Domain.Entities;

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
}

public class LedgerAccount : BaseEntity
{
    public Guid? VendorId { get; set; } // Null if it is a platform account (e.g., Platform Commission Revenue)
    
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    
    public decimal CurrentBalance { get; set; } = 0m;
}
