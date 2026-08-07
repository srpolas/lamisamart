using Microsoft.EntityFrameworkCore;
using LamisaMart.Payments.Domain.Entities;

namespace LamisaMart.Payments.Application.Common.Interfaces;

public interface IPaymentsDbContext
{
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
