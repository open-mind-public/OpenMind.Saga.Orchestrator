using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using OpenMind.Order.Application.Queries.GetOrder;
using IntegrationOrderItemDto = OpenMind.BuildingBlocks.IntegrationEvents.Orders.OrderItemDto;

namespace OpenMind.Order.Infrastructure.Consumers;

/// <summary>
/// Consumer for ValidateOrderCommand from the orchestrator.
/// Validates that an order exists and returns its details.
/// </summary>
public class ValidateOrderCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    : IConsumer<ValidateOrderCommand>
{
    public async Task Consume(ConsumeContext<ValidateOrderCommand> context)
    {
        var query = new GetOrderQuery(context.Message.OrderId);
        var result = await mediator.Send(query);

        if (result.IsSuccess && result.Data is not null)
        {
            var order = result.Data;
            await publishEndpoint.Publish(new OrderValidatedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CustomerEmail = $"customer-{order.CustomerId}@example.com", // Could be stored in Order or Customer service
                CustomerName = $"Customer {order.CustomerId.ToString()[..8]}", // Could be stored in Order or Customer service
                Items = order.Items.Select(i => new IntegrationOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });
        }
        else
        {
            await publishEndpoint.Publish(new OrderValidationFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                Reason = result.ErrorMessage ?? "Order not found"
            });
        }
    }
}
