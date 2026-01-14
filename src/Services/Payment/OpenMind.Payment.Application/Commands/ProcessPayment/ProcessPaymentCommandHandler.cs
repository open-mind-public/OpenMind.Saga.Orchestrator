using Microsoft.Extensions.Logging;
using OpenMind.Payment.Domain.Repositories;
using OpenMind.Shared.Application.Commands;

namespace OpenMind.Payment.Application.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler(IPaymentRepository paymentRepository, ILogger<ProcessPaymentCommandHandler> logger)
    : ICommandHandler<ProcessPaymentCommand, Guid>
{
    public async Task<CommandResult<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("[ProcessPayment] Creating payment for OrderId: {OrderId}", request.OrderId);
            
            // Create payment
            var payment = Domain.Aggregates.Payment.Create(
                request.OrderId,
                request.CustomerId,
                request.Amount,
                request.PaymentMethod,
                request.CardNumber);

            logger.LogDebug("[ProcessPayment] Payment created: {PaymentId}, Events: {EventCount}", 
                payment.Id, payment.DomainEvents.Count);

            // Mark as processing - this publishes PaymentProcessingStartedDomainEvent
            // The domain event handler will call the payment gateway
            payment.MarkAsProcessing(request.CardNumber, request.CardExpiry);

            logger.LogDebug("[ProcessPayment] Payment marked as processing, Events: {EventCount}", 
                payment.DomainEvents.Count);

            await paymentRepository.AddAsync(payment, cancellationToken);

            logger.LogDebug("[ProcessPayment] Payment saved to repository");

            return CommandResult<Guid>.Success(payment.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ProcessPayment] ERROR: {Message}", ex.Message);
            return CommandResult<Guid>.Failure(ex.Message, "PAYMENT_ERROR");
        }
    }
}
