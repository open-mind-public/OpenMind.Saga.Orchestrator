using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Payment.Domain.Repositories;

namespace OpenMind.Payment.Application.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler(IPaymentRepository paymentRepository)
    : ICommandHandler<ProcessPaymentCommand, Guid>
{
    public async Task<CommandResult<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create payment
            var payment = Domain.Aggregates.Payment.Create(
                request.OrderId,
                request.CustomerId,
                request.Amount,
                request.PaymentMethod,
                request.CardNumber);

            // Mark as processing - this publishes PaymentProcessingStartedDomainEvent
            // The domain event handler will call the payment gateway
            payment.MarkAsProcessing(request.CardNumber, request.CardExpiry);

            await paymentRepository.AddAsync(payment, cancellationToken);

            return CommandResult<Guid>.Success(payment.Id);
        }
        catch (Exception ex)
        {
            return CommandResult<Guid>.Failure(ex.Message, "PAYMENT_ERROR");
        }
    }
}
