using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using AppCommand = OpenMind.Order.Application.Commands.CancelOrder;

namespace OpenMind.Order.Infrastructure.Consumers;

/// <summary>
/// Consumer for CancelOrderCommand from the orchestrator.
/// </summary>
public class CancelOrderCommandConsumer(IMediator mediator) : IConsumer<CancelOrderCommand>
{
    public async Task Consume(ConsumeContext<CancelOrderCommand> context)
    {
        var command = new AppCommand.CancelOrderCommand
        {
            OrderId = context.Message.OrderId,
            Reason = context.Message.Reason,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);
    }
}
