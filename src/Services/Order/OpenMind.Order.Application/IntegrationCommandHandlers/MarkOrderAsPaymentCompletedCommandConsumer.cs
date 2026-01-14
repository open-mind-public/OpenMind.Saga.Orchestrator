using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Order.IntegrationEvents.Commands;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsPaymentCompleted;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

public class MarkOrderAsPaymentCompletedCommandConsumer(IMediator mediator, ILogger<MarkOrderAsPaymentCompletedCommandConsumer> logger)
    : IConsumer<MarkOrderAsPaymentCompletedCommand>
{
    public async Task Consume(ConsumeContext<MarkOrderAsPaymentCompletedCommand> context)
    {
        var command = new AppCommand.MarkOrderAsPaymentCompletedCommand
        {
            OrderId = context.Message.OrderId,
            TransactionId = context.Message.TransactionId,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);

        logger.LogInformation("[Order] Consumed MarkOrderAsPaymentCompletedCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Message.CorrelationId);
    }
}
