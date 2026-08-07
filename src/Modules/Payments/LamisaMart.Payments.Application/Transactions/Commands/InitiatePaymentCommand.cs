using MediatR;
using LamisaMart.Payments.Application.Common.Interfaces;
using LamisaMart.Payments.Domain.Entities;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Payments.Application.Transactions.Commands;

public record InitiatePaymentCommand(
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string Currency,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string GatewayName = "SSLCommerz"
) : IRequest<InitiatePaymentResultDto>;

public record InitiatePaymentResultDto(bool Success, string GatewayUrl, string TransactionId, string ErrorMessage = "");

public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResultDto>
{
    private readonly IPaymentsDbContext _context;
    private readonly ISSLCommerzClient _sslClient;

    public InitiatePaymentCommandHandler(IPaymentsDbContext context, ISSLCommerzClient sslClient)
    {
        _context = context;
        _sslClient = sslClient;
    }

    public async Task<InitiatePaymentResultDto> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var transaction = new PaymentTransaction
        {
            OrderId = request.OrderId,
            OrderNumber = request.OrderNumber,
            Amount = new Money(request.Amount, request.Currency),
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            GatewayName = request.GatewayName,
            Status = PaymentStatus.Pending,
            TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}"
        };

        _context.PaymentTransactions.Add(transaction);
        
        InitiatePaymentResult result;
        
        if (request.GatewayName == "SSLCommerz")
        {
            result = await _sslClient.InitiatePaymentAsync(transaction, cancellationToken);
            if (result.Success)
            {
                transaction.SessionKey = result.SessionKey;
                await _context.SaveChangesAsync(cancellationToken);
                return new InitiatePaymentResultDto(true, result.GatewayUrl, transaction.TransactionId);
            }
        }
        else if (request.GatewayName == "BanglaQR")
        {
            // Dummy logic for BanglaQR for now
            transaction.Status = PaymentStatus.Success; // Just for testing
            await _context.SaveChangesAsync(cancellationToken);
            return new InitiatePaymentResultDto(true, "/checkout/success", transaction.TransactionId);
        }
        else
        {
            return new InitiatePaymentResultDto(false, "", "", "Unsupported gateway");
        }

        transaction.Status = PaymentStatus.Failed;
        await _context.SaveChangesAsync(cancellationToken);
        return new InitiatePaymentResultDto(false, "", "", result?.ErrorMessage ?? "Failed to initiate payment");
    }
}
