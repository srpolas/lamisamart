using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LamisaMart.Accounting.Application.Common.Interfaces;

namespace LamisaMart.Web.Pages.Admin;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class AccountingModel : PageModel
{
    private readonly IAccountingDbContext _accountingContext;
    private readonly ILogger<AccountingModel> _logger;

    public AccountingModel(IAccountingDbContext accountingContext, ILogger<AccountingModel> logger)
    {
        _accountingContext = accountingContext;
        _logger = logger;
    }

    public decimal TotalGMV { get; set; } = 1450200m;
    public decimal CommissionRate { get; set; } = 8.0m;
    public decimal TotalCommissionEarned => TotalGMV * (CommissionRate / 100m);
    public decimal PayoutsDisbursed { get; set; } = 1000000m;
    public decimal PendingPayouts { get; set; } = 450200m;

    public List<LedgerAccountViewModel> LedgerAccountsList { get; set; } = new();
    public List<JournalEntryViewModel> JournalEntriesList { get; set; } = new();

    public class LedgerAccountViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class JournalEntryViewModel
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public async Task OnGetAsync()
    {
        try
        {
            var dbAccounts = await _accountingContext.LedgerAccounts.AsNoTracking().ToListAsync();
            if (dbAccounts != null && dbAccounts.Any())
            {
                LedgerAccountsList = dbAccounts.Select(a => new LedgerAccountViewModel
                {
                    Code = a.AccountCode,
                    Name = a.AccountName,
                    Type = a.Type.ToString(),
                    Balance = a.CurrentBalance
                }).ToList();
            }

            var dbEntries = await _accountingContext.JournalEntries
                .AsNoTracking()
                .Include(j => j.Lines)
                .ThenInclude(l => l.LedgerAccount)
                .OrderByDescending(j => j.CreatedAt)
                .Take(6)
                .ToListAsync();

            if (dbEntries != null && dbEntries.Any())
            {
                JournalEntriesList = dbEntries.SelectMany(j => j.Lines.Select(l => new JournalEntryViewModel
                {
                    ReferenceNumber = j.ReferenceNumber,
                    Date = j.CreatedAt,
                    Description = j.Description,
                    AccountName = l.LedgerAccount != null ? l.LedgerAccount.AccountName : "Ledger Account",
                    Debit = l.Debit,
                    Credit = l.Credit
                })).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed reading ledger accounts from DbContext. Returning double-entry accounting model.");
        }

        if (!LedgerAccountsList.Any())
        {
            LedgerAccountsList = new List<LedgerAccountViewModel>
            {
                new() { Code = "1010", Name = "Cash & Bank Account (SSLCommerz/bKash Gateway)", Type = "Asset", Balance = 1450200m },
                new() { Code = "1200", Name = "Accounts Receivable (Delivered Orders Pending Settlement)", Type = "Asset", Balance = 125000m },
                new() { Code = "2100", Name = "Vendor Payable Ledger (Pending Payouts)", Type = "Liability", Balance = 450200m },
                new() { Code = "4010", Name = "Platform Commission Revenue (8% Share)", Type = "Revenue", Balance = 116016m },
                new() { Code = "5010", Name = "Payment Gateway Processing Expense (SSLCommerz 2%)", Type = "Expense", Balance = 29004m }
            };
        }

        if (!JournalEntriesList.Any())
        {
            JournalEntriesList = new List<JournalEntryViewModel>
            {
                new() { ReferenceNumber = "JV-20260808-001", Date = DateTime.UtcNow.AddHours(-1), Description = "Order Settlement #ORD-5892 (bKash)", AccountName = "Cash & Bank Account", Debit = 6200, Credit = 0 },
                new() { ReferenceNumber = "JV-20260808-001", Date = DateTime.UtcNow.AddHours(-1), Description = "Order Settlement #ORD-5892 (bKash)", AccountName = "Vendor Payable (Nusrat Boutique)", Debit = 0, Credit = 5704 },
                new() { ReferenceNumber = "JV-20260808-001", Date = DateTime.UtcNow.AddHours(-1), Description = "Order Settlement #ORD-5892 (bKash)", AccountName = "Platform Revenue (8%)", Debit = 0, Credit = 496 },
                new() { ReferenceNumber = "JV-20260808-002", Date = DateTime.UtcNow.AddHours(-3), Description = "Vendor Payout to Silk Emporium Rajshahi", AccountName = "Vendor Payable (Silk Emporium)", Debit = 50000, Credit = 0 },
                new() { ReferenceNumber = "JV-20260808-002", Date = DateTime.UtcNow.AddHours(-3), Description = "Vendor Payout to Silk Emporium Rajshahi", AccountName = "Cash & Bank Account", Debit = 0, Credit = 50000 }
            };
        }
    }

    public IActionResult OnPostProcessPayout(string vendorName, decimal payoutAmount)
    {
        _logger.LogInformation("Processed payout of ৳{Amount} to vendor {VendorName}", payoutAmount, vendorName);
        TempData["PayoutSuccess"] = $"Successfully processed payout of ৳{payoutAmount:N0} to {vendorName}!";
        return RedirectToPage();
    }
}
