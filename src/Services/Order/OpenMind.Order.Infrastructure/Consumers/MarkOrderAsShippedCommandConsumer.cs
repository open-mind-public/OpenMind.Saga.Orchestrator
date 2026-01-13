using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsShipped;

namespace OpenMind.Order.Infrastructure.Consumers;

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
