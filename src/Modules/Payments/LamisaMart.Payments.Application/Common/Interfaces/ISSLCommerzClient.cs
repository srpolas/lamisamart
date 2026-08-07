using LamisaMart.Payments.Domain.Entities;

namespace LamisaMart.Payments.Application.Common.Interfaces;

public record InitiatePaymentResult(bool Success, string GatewayUrl, string SessionKey, string ErrorMessage = "");
public record ValidatePaymentResult(bool IsValid, string ValidatedAmount, string ErrorMessage = "");

public interface ISSLCommerzClient
{
    Task<InitiatePaymentResult> InitiatePaymentAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);
    Task<ValidatePaymentResult> ValidatePaymentAsync(string valId, CancellationToken cancellationToken = default);
}
