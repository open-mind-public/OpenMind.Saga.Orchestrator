using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Order.Contract.Commands;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsShipped;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

public class MarkOrderAsShippedCommandConsumer(IMediator mediator, ILogger<MarkOrderAsShippedCommandConsumer> logger)
    : IConsumer<MarkOrderAsShippedCommand>
{
    public async Task Consume(ConsumeContext<MarkOrderAsShippedCommand> context)
    {
        var command = new AppCommand.MarkOrderAsShippedCommand
        {
            OrderId = context.Message.OrderId,
            TrackingNumber = context.Message.TrackingNumber,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);

        logger.LogInformation("[Order] Consumed MarkOrderAsShippedCommand - OrderId: {OrderId}, TrackingNumber: {TrackingNumber}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Message.TrackingNumber, context.Message.CorrelationId);
    }
}
