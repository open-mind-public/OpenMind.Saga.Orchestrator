using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Fulfillment;
using AppCommand = OpenMind.Fulfillment.Application.Commands.FulfillOrder;

namespace OpenMind.Fulfillment.Infrastructure.Consumers;

public class FulfillOrderCommandConsumer : IConsumer<FulfillOrderCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public FulfillOrderCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<FulfillOrderCommand> context)
    {
        var command = new AppCommand.FulfillOrderCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            Items = context.Message.Items.Select(i => new AppCommand.FulfillmentItemCommand(
                i.ProductId,
                i.ProductName,
                i.Quantity)).ToList(),
            ShippingAddress = context.Message.ShippingAddress
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            await _publishEndpoint.Publish(new OrderShippedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                FulfillmentId = result.Data.FulfillmentId,
                TrackingNumber = result.Data.TrackingNumber ?? string.Empty,
                EstimatedDelivery = DateTime.UtcNow.AddDays(5)
            });
        }
        else
        {
            // Check if it's a backorder situation
            await _publishEndpoint.Publish(new FulfillmentFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                Reason = result.ErrorMessage ?? "Fulfillment failed",
                OutOfStockItems = context.Message.Items.Select(i => new OutOfStockItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    RequestedQuantity = i.Quantity,
                    AvailableQuantity = 0
                }).ToList()
            });
        }
    }
}
