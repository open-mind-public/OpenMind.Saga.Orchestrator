using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Payment.IntegrationEvents.Commands;
using AppCommand = OpenMind.Payment.Application.Commands.ProcessPayment;

namespace OpenMind.Payment.Application.IntegrationCommandHandlers;

public class ProcessPaymentCommandConsumer(IMediator mediator, ILogger<ProcessPaymentCommandConsumer> logger) 
    : IConsumer<ProcessPaymentCommand>
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

        await mediator.Send(command);

        logger.LogInformation("[Payment] Consumed ProcessPaymentCommand - OrderId: {OrderId}, Amount: {Amount}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Message.Amount, context.Message.CorrelationId);
    }
}
