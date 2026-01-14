using Microsoft.Extensions.Logging;
using OpenMind.Payment.Domain.Repositories;
using OpenMind.Shared.Application.Commands;

namespace OpenMind.Payment.Application.Commands.RefundPayment;

public class RefundPaymentCommandHandler(IPaymentRepository paymentRepository, ILogger<RefundPaymentCommandHandler> logger)
    : ICommandHandler<RefundPaymentCommand>
{
    public async Task<CommandResult> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
                ?? await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (payment is null)
                return CommandResult.Failure($"Payment not found for order {request.OrderId}", "PAYMENT_NOT_FOUND");

            // Simulate refund processing (95% success rate)
            var refundSuccess = Random.Shared.Next(100) < 95;

            if (refundSuccess)
            {
                payment.MarkAsRefunded(request.CorrelationId);
                await paymentRepository.UpdateAsync(payment, cancellationToken);
                return CommandResult.Success();
            }
            else
            {
                payment.MarkAsRefundFailed("Refund processing error", request.CorrelationId);
                await paymentRepository.UpdateAsync(payment, cancellationToken);
                return CommandResult.Failure("Refund failed", "REFUND_ERROR");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[RefundPayment] ERROR: {Message}", ex.Message);
            return CommandResult.Failure(ex.Message, "REFUND_ERROR");
        }
    }
}
