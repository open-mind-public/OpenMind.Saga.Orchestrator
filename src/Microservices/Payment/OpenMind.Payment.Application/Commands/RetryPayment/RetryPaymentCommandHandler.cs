using Microsoft.Extensions.Logging;
using OpenMind.Payment.Domain.Repositories;
using OpenMind.Shared.Application.Commands;

namespace OpenMind.Payment.Application.Commands.RetryPayment;

public class RetryPaymentCommandHandler(IPaymentRepository paymentRepository, ILogger<RetryPaymentCommandHandler> logger)
    : ICommandHandler<RetryPaymentCommand, bool>
{
    public async Task<CommandResult<bool>> Handle(RetryPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (payment == null)
            {
                return CommandResult<bool>.Failure($"Payment for order {request.OrderId} not found", "PAYMENT_NOT_FOUND");
            }

            payment.Retry();

            await paymentRepository.UpdateAsync(payment, cancellationToken);

            logger.LogInformation("[RetryPayment] Payment {PaymentId} for Order {OrderId} retried", payment.Id, request.OrderId);

            return CommandResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[RetryPayment] ERROR: {Message}", ex.Message);
            return CommandResult<bool>.Failure(ex.Message, "PAYMENT_ERROR");
        }
    }
}
