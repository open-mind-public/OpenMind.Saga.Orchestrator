using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Order.Contract.Commands;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsBackOrdered;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

public class MarkOrderAsBackOrderedCommandConsumer(IMediator mediator, ILogger<MarkOrderAsBackOrderedCommandConsumer> logger)
    : IConsumer<MarkOrderAsBackOrderedCommand>
{
    public async Task Consume(ConsumeContext<MarkOrderAsBackOrderedCommand> context)
    {
        var command = new AppCommand.MarkOrderAsBackOrderedCommand
        {
            OrderId = context.Message.OrderId,
            Reason = context.Message.Reason,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);

        logger.LogInformation("[Order] Consumed MarkOrderAsBackOrderedCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Message.CorrelationId);
    }
}
