using Financial.Contracts;
using Financial.Contracts.DTOs;

namespace Financial.Core.Gateways;

public class ManualPaymentGateway : IPaymentGateway
{
    public string ProviderName => "manual";

    public Task<PaymentGatewayResult> InitiatePaymentAsync(PaymentGatewayRequest request, CancellationToken ct = default)
    {
        return Task.FromResult(new PaymentGatewayResult
        {
            Success = true,
            TransactionId = $"MANUAL-{Guid.NewGuid():N}"[..24],
            Status = "completed",
        });
    }

    public Task<PaymentGatewayResult> ConfirmPaymentAsync(string transactionId, CancellationToken ct = default)
    {
        return Task.FromResult(new PaymentGatewayResult
        {
            Success = true,
            TransactionId = transactionId,
            Status = "completed",
        });
    }

    public Task<PaymentGatewayResult> RefundAsync(string transactionId, decimal amount, CancellationToken ct = default)
    {
        return Task.FromResult(new PaymentGatewayResult
        {
            Success = true,
            TransactionId = transactionId,
            Status = "refunded",
        });
    }

    public Task<PaymentGatewayResult> GetStatusAsync(string transactionId, CancellationToken ct = default)
    {
        return Task.FromResult(new PaymentGatewayResult
        {
            Success = true,
            TransactionId = transactionId,
            Status = "completed",
        });
    }
}
