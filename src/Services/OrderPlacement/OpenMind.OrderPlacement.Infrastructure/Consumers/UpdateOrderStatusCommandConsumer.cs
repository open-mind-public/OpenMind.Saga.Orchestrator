using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using AppCommand = OpenMind.OrderPlacement.Application.Commands.UpdateOrderStatus;

namespace OpenMind.OrderPlacement.Infrastructure.Consumers;

/// <summary>
/// Consumer for UpdateOrderStatusCommand from the orchestrator.
/// </summary>
public class UpdateOrderStatusCommandConsumer : IConsumer<UpdateOrderStatusCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateOrderStatusCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<UpdateOrderStatusCommand> context)
    {
        var command = new AppCommand.UpdateOrderStatusCommand
        {
            OrderId = context.Message.OrderId,
            Status = context.Message.Status,
            Reason = context.Message.Reason
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            await _publishEndpoint.Publish(new OrderStatusUpdatedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                Status = context.Message.Status
            });
        }
    }
}
