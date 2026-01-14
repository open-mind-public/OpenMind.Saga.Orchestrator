using MediatR;
using OpenMind.Payment.Application.Commands.MarkPaymentAsFailed;
using OpenMind.Payment.Application.Commands.MarkPaymentAsPaid;
using OpenMind.Payment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Payment.Application.DomainEventHandlers;

/// <summary>
/// Handles the PaymentProcessingStartedDomainEvent.
/// Calls the payment gateway and dispatches appropriate commands based on result.
/// </summary>
public class PaymentProcessingStartedDomainEventHandler(IMediator mediator)
    : IDomainEventHandler<PaymentProcessingStartedDomainEvent>
{
    public async Task Handle(DomainEventNotification<PaymentProcessingStartedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Simulate payment processing with card validation
        var isValidCard = SimulatePaymentGateway(domainEvent.CardNumber, domainEvent.CardExpiry);

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
        if (cardNumber.EndsWith("0000"))
            return false;

        if (!string.IsNullOrEmpty(expiry))
        {
            var parts = expiry.Split('/');
            if (parts.Length == 2 && int.TryParse(parts[1], out var year))
            {
                var currentYear = DateTime.UtcNow.Year % 100;
                if (year < currentYear)
                    return false;
            }
        }

        // Simulate 90% success rate
        return Random.Shared.Next(100) < 90;
    }
}
