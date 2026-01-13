using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Fulfillment;
using AppCommand = OpenMind.Fulfillment.Application.Commands.CancelFulfillment;

namespace OpenMind.Fulfillment.Infrastructure.Consumers;

public class CancelFulfillmentCommandConsumer : IConsumer<CancelFulfillmentCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelFulfillmentCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<CancelFulfillmentCommand> context)
    {
        var command = new AppCommand.CancelFulfillmentCommand
        {
            OrderId = context.Message.OrderId,
            FulfillmentId = context.Message.FulfillmentId,
            Reason = context.Message.Reason
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            await _publishEndpoint.Publish(new FulfillmentCancelledEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                FulfillmentId = context.Message.FulfillmentId
            });
        }
    }
}
