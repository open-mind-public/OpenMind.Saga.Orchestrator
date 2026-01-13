using MassTransit;
using MediatR;
using OpenMind.Order.IntegrationEvents.Commands;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsShipped;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

public class MarkOrderAsShippedCommandConsumer(IMediator mediator)
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
    }
}
