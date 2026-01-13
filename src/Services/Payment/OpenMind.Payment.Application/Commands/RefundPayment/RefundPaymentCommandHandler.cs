using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Payment.Domain.Repositories;

namespace OpenMind.Payment.Application.Commands.RefundPayment;

public class RefundPaymentCommandHandler(IPaymentRepository paymentRepository)
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
            return CommandResult.Failure(ex.Message, "REFUND_ERROR");
        }
    }
}
