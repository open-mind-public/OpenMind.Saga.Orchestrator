using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsBackOrdered;

namespace OpenMind.Order.Infrastructure.Consumers;

public class MarkOrderAsBackOrderedCommandConsumer(IMediator mediator)
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
    }
}
