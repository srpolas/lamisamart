using MediatR;
using Microsoft.EntityFrameworkCore;
using LamisaMart.Accounting.Application.Common.Interfaces;
using LamisaMart.Accounting.Domain.Entities;

namespace LamisaMart.Accounting.Application.Journal.Commands;

public record RecordSubOrderAccountingCommand(
    string ReferenceNumber,
    Guid VendorId,
    decimal SubTotal,
    decimal CommissionAmount,
    decimal VendorPayoutAmount
) : IRequest<bool>;

public class RecordSubOrderAccountingCommandHandler : IRequestHandler<RecordSubOrderAccountingCommand, bool>
{
    private readonly IAccountingDbContext _context;

    public RecordSubOrderAccountingCommandHandler(IAccountingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RecordSubOrderAccountingCommand request, CancellationToken cancellationToken)
    {
        // For a Double-Entry Ledger, we need accounts. Usually these are seeded.
        // E.g.
        // 1. Accounts Receivable (Platform)
        // 2. Platform Commission Revenue (Platform)
        // 3. Vendor Payable (Liability to Vendor)
        
        var accounts = await _context.LedgerAccounts.ToListAsync(cancellationToken);
        
        var arAccount = accounts.FirstOrDefault(a => a.AccountCode == "1000"); // AR
        var commissionAccount = accounts.FirstOrDefault(a => a.AccountCode == "4000"); // Rev
        var payableAccount = accounts.FirstOrDefault(a => a.VendorId == request.VendorId && a.AccountCode == "2000"); // Vendor Payable
        
        // If accounts don't exist in this basic stub, we'd create them or seed them first.
        if (arAccount == null || commissionAccount == null)
            return false;
            
        if (payableAccount == null)
        {
            payableAccount = new LedgerAccount
            {
                VendorId = request.VendorId,
                AccountCode = "2000",
                AccountName = "Vendor Payable",
                Type = AccountType.Liability
            };
            _context.LedgerAccounts.Add(payableAccount);
        }

        var entry = new JournalEntry
        {
            ReferenceNumber = request.ReferenceNumber,
            Description = $"Sales recording for Sub-Order {request.ReferenceNumber}",
            EntryDate = DateTime.UtcNow
        };

        // Debit AR for the full subtotal (customer owes us or paid us)
        entry.Lines.Add(new JournalEntryLine
        {
            LedgerAccount = arAccount,
            Debit = request.SubTotal,
            Credit = 0m
        });

        // Credit Commission Revenue for platform's cut
        entry.Lines.Add(new JournalEntryLine
        {
            LedgerAccount = commissionAccount,
            Debit = 0m,
            Credit = request.CommissionAmount
        });

        // Credit Vendor Payable for the vendor's net payout
        entry.Lines.Add(new JournalEntryLine
        {
            LedgerAccount = payableAccount,
            Debit = 0m,
            Credit = request.VendorPayoutAmount
        });
        
        // The ledger is balanced: Debit = SubTotal, Credit = Commission (10%) + Payout (90%)

        _context.JournalEntries.Add(entry);
        
        // Update Running Balances (Simplified for example)
        arAccount.CurrentBalance += request.SubTotal; // Asset increases with Debit
        commissionAccount.CurrentBalance += request.CommissionAmount; // Revenue increases with Credit
        payableAccount.CurrentBalance += request.VendorPayoutAmount; // Liability increases with Credit

        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
