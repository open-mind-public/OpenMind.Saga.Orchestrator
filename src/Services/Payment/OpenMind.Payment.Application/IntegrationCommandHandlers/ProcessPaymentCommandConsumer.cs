using MassTransit;
using MediatR;
using OpenMind.Payment.IntegrationEvents.Commands;
using AppCommand = OpenMind.Payment.Application.Commands.ProcessPayment;

namespace OpenMind.Payment.Application.IntegrationCommandHandlers;

public class ProcessPaymentCommandConsumer(IMediator mediator) : IConsumer<ProcessPaymentCommand>
{
    public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
    {
        var command = new AppCommand.ProcessPaymentCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            Amount = context.Message.Amount,
            PaymentMethod = context.Message.PaymentMethod,
            CardNumber = context.Message.CardNumber,
            CardExpiry = context.Message.CardExpiry
        };

        // The command handler will trigger domain events which will:
        // 1. PaymentProcessingStartedDomainEvent -> calls payment gateway -> dispatches MarkPaymentAsPaid/MarkPaymentAsFailed command
        // 2. PaymentPaidDomainEvent -> publishes PaymentCompletedEvent integration event
        // 3. PaymentFailedDomainEvent -> publishes PaymentFailedEvent integration event
        await mediator.Send(command);
    }
}
