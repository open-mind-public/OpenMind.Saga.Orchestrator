using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using AppCommand = OpenMind.OrderPlacement.Application.Commands.CancelOrder;

namespace OpenMind.OrderPlacement.Infrastructure.Consumers;

/// <summary>
/// Consumer for CancelOrderCommand from the orchestrator.
/// </summary>
public class CancelOrderCommandConsumer : IConsumer<CancelOrderCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelOrderCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<CancelOrderCommand> context)
    {
        var command = new AppCommand.CancelOrderCommand
        {
            OrderId = context.Message.OrderId,
            Reason = context.Message.Reason
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            await _publishEndpoint.Publish(new OrderCancelledEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId
            });
        }
    }
}
