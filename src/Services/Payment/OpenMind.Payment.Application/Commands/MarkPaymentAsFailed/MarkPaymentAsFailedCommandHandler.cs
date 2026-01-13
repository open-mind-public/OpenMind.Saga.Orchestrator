using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Payment.Domain.Repositories;

namespace OpenMind.Payment.Application.Commands.MarkPaymentAsFailed;

public class MarkPaymentAsFailedCommandHandler(IPaymentRepository paymentRepository)
    : ICommandHandler<MarkPaymentAsFailedCommand, bool>
{
    public async Task<CommandResult<bool>> Handle(MarkPaymentAsFailedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null)
            {
                return CommandResult<bool>.Failure($"Payment {request.PaymentId} not found", "PAYMENT_NOT_FOUND");
            }

            // Mark as failed - this publishes PaymentFailedDomainEvent
            payment.MarkAsFailed(request.Reason);

            await paymentRepository.UpdateAsync(payment, cancellationToken);

            return CommandResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return CommandResult<bool>.Failure(ex.Message, "PAYMENT_ERROR");
        }
    }
}
