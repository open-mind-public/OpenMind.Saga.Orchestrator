using Microsoft.Extensions.Logging;
using OpenMind.Payment.Domain.Repositories;
using OpenMind.Shared.Application.Commands;

namespace OpenMind.Payment.Application.Commands.MarkPaymentAsFailed;

public class MarkPaymentAsFailedCommandHandler(IPaymentRepository paymentRepository, ILogger<MarkPaymentAsFailedCommandHandler> logger)
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

            payment.MarkAsFailed(request.Reason);

            await paymentRepository.UpdateAsync(payment, cancellationToken);

            return CommandResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[MarkPaymentAsFailed] ERROR: {Message}", ex.Message);
            return CommandResult<bool>.Failure(ex.Message, "PAYMENT_ERROR");
        }
    }
}
