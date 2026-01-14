using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Order.IntegrationEvents.Commands;
using OpenMind.Order.IntegrationEvents.Events;
using OpenMind.Order.Application.Queries.GetOrder;
using IntegrationOrderItemDto = OpenMind.Order.IntegrationEvents.OrderItemDto;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

/// <summary>
/// Consumer for ValidateOrderCommand from the orchestrator.
/// Validates that an order exists and returns its details.
/// </summary>
public class ValidateOrderCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint, ILogger<ValidateOrderCommandConsumer> logger)
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
                CustomerEmail = $"customer-{order.CustomerId}@example.com",
                CustomerName = $"Customer {order.CustomerId.ToString()[..8]}",
                Items = order.Items.Select(i => new IntegrationOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });

            logger.LogInformation("[Order] Published OrderValidatedEvent - OrderId: {OrderId}, CorrelationId: {CorrelationId}", order.Id, context.Message.CorrelationId);
        }
        else
        {
            await publishEndpoint.Publish(new OrderValidationFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                Reason = result.ErrorMessage ?? "Order not found"
            });

            logger.LogWarning("[Order] Published OrderValidationFailedEvent - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.OrderId, result.ErrorMessage ?? "Order not found", context.Message.CorrelationId);
        }
    }
}
