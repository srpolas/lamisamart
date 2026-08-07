using MediatR;
using Microsoft.EntityFrameworkCore;
using LamisaMart.Payments.Application.Common.Interfaces;
using LamisaMart.Payments.Domain.Entities;

namespace LamisaMart.Payments.Application.Transactions.Commands;

public record ValidatePaymentCommand(
    string ValId,
    string TransactionId,
    string Status // From IPN/Callback (e.g. "VALID", "FAILED", "CANCELLED")
) : IRequest<bool>;

public class ValidatePaymentCommandHandler : IRequestHandler<ValidatePaymentCommand, bool>
{
    private readonly IPaymentsDbContext _context;
    private readonly ISSLCommerzClient _sslClient;

    public ValidatePaymentCommandHandler(IPaymentsDbContext context, ISSLCommerzClient sslClient)
    {
        _context = context;
        _sslClient = sslClient;
    }

    public async Task<bool> Handle(ValidatePaymentCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.PaymentTransactions
            .FirstOrDefaultAsync(t => t.TransactionId == request.TransactionId, cancellationToken);

        if (transaction == null || transaction.Status == PaymentStatus.Success)
        {
            return false; // Already processed or not found
        }

        if (request.Status == "VALID" || request.Status == "VALIDATED")
        {
            var result = await _sslClient.ValidatePaymentAsync(request.ValId, cancellationToken);
            
            if (result.IsValid)
            {
                // Optionally verify the amount matches
                transaction.Status = PaymentStatus.Success;
                transaction.GatewayTransactionId = request.ValId;
                transaction.PaidAt = DateTime.UtcNow;
                
                // Here we would typically publish an Event (e.g., PaymentCompletedEvent)
                // so the Ordering module can mark the Order as Paid.
                
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
        else if (request.Status == "FAILED" || request.Status == "CANCELLED")
        {
            transaction.Status = request.Status == "CANCELLED" ? PaymentStatus.Cancelled : PaymentStatus.Failed;
            transaction.GatewayTransactionId = request.ValId;
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }

        return false;
    }
}
