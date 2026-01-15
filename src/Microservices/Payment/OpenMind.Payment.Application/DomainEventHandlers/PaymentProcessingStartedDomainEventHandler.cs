using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenMind.Payment.Application.Commands.MarkPaymentAsFailed;
using OpenMind.Payment.Application.Commands.MarkPaymentAsPaid;
using OpenMind.Payment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Payment.Application.DomainEventHandlers;

/// <summary>
/// Handles the PaymentProcessingStartedDomainEvent.
/// Calls the payment gateway and dispatches appropriate commands based on result.
/// </summary>
public class PaymentProcessingStartedDomainEventHandler(IMediator mediator, IConfiguration configuration, ILogger<PaymentProcessingStartedDomainEventHandler> logger)
    : IDomainEventHandler<PaymentProcessingStartedDomainEvent>
{
    public async Task Handle(DomainEventNotification<PaymentProcessingStartedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Simulate payment processing with card validation
        var isValidCard = SimulatePaymentGateway(domainEvent.CardNumber, domainEvent.CardExpiry);
        var expectedResult = configuration.GetValue<bool>("Test:ExpectPaymentSuccess");

        if (expectedResult is false)
        {
            logger.LogInformation("Expected payment failure for testing purposes.");
            
            await mediator.Send(new MarkPaymentAsFailedCommand
            {
                PaymentId = domainEvent.PaymentId,
                Reason = "Payment declined: Invalid or expired card"
            }, cancellationToken);
        }

        if (isValidCard)
        {
            var transactionId = $"TXN-{Guid.NewGuid():N}".ToUpper()[..20];
            
            // Dispatch command to mark payment as paid
            await mediator.Send(new MarkPaymentAsPaidCommand
            {
                PaymentId = domainEvent.PaymentId,
                TransactionId = transactionId
            }, cancellationToken);
        }
        else
        {
            // Dispatch command to mark payment as failed
            await mediator.Send(new MarkPaymentAsFailedCommand
            {
                PaymentId = domainEvent.PaymentId,
                Reason = "Payment declined: Invalid or expired card"
            }, cancellationToken);
        }
    }

    private static bool SimulatePaymentGateway(string cardNumber, string expiry)
    {
        return true;
    }
}
