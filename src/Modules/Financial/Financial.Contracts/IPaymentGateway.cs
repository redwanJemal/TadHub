using Financial.Contracts.DTOs;

namespace Financial.Contracts;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<PaymentGatewayResult> InitiatePaymentAsync(PaymentGatewayRequest request, CancellationToken ct = default);
    Task<PaymentGatewayResult> ConfirmPaymentAsync(string transactionId, CancellationToken ct = default);
    Task<PaymentGatewayResult> RefundAsync(string transactionId, decimal amount, CancellationToken ct = default);
    Task<PaymentGatewayResult> GetStatusAsync(string transactionId, CancellationToken ct = default);
}
