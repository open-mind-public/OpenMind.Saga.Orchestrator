using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Payment.Domain.Repositories;

namespace OpenMind.Payment.Application.Commands.MarkPaymentAsPaid;

public class MarkPaymentAsPaidCommandHandler(IPaymentRepository paymentRepository)
    : ICommandHandler<MarkPaymentAsPaidCommand, bool>
{
    public async Task<CommandResult<bool>> Handle(MarkPaymentAsPaidCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null)
            {
                return CommandResult<bool>.Failure($"Payment {request.PaymentId} not found", "PAYMENT_NOT_FOUND");
            }

            payment.MarkAsPaid(request.TransactionId);

            await paymentRepository.UpdateAsync(payment, cancellationToken);

            return CommandResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return CommandResult<bool>.Failure(ex.Message, "PAYMENT_ERROR");
        }
    }
}
