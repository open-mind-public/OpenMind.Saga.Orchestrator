using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Order.Application.Queries.GetOrder;
using OpenMind.Order.Contract.Commands;
using OpenMind.Order.Contract.Events;
using IntegrationOrderItemDto = OpenMind.Order.Contract.OrderItemDto;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

/// <summary>
/// Consumer for ValidateOrderCommand from the orchestrator.
/// Validates that an order exists and returns its details.
/// </summary>
public class ValidateOrderCommandConsumer(IMediator mediator, ILogger<ValidateOrderCommandConsumer> logger)
    : IConsumer<ValidateOrderCommand>
{
    public async Task Consume(ConsumeContext<ValidateOrderCommand> context)
    {
        // Debug: Log incoming message details
        logger.LogDebug("[Order] Received ValidateOrderCommand - OrderId: {OrderId}, Message.CorrelationId: {MessageCorrelationId}, Header.CorrelationId: {HeaderCorrelationId}",
            context.Message.OrderId,
            context.Message.CorrelationId,
            context.CorrelationId);

        var query = new GetOrderQuery(context.Message.OrderId);
        var result = await mediator.Send(query);

        if (result.IsSuccess && result.Data is not null)
        {
            var order = result.Data;
            // Use context.CorrelationId (header) to ensure proper saga correlation
            var correlationId = context.CorrelationId ?? context.Message.CorrelationId;
            
            await context.Publish(new OrderValidatedEvent
            {
                CorrelationId = correlationId,
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

            logger.LogInformation("[Order] Published OrderValidatedEvent - OrderId: {OrderId}, CorrelationId: {CorrelationId}", order.Id, correlationId);
        }
        else
        {
            // Use context.CorrelationId (header) to ensure proper saga correlation
            var correlationId = context.CorrelationId ?? context.Message.CorrelationId;
            
            await context.Publish(new OrderValidationFailedEvent
            {
                CorrelationId = correlationId,
                OrderId = context.Message.OrderId,
                Reason = result.ErrorMessage ?? "Order not found"
            });

            logger.LogWarning("[Order] Published OrderValidationFailedEvent - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.OrderId, result.ErrorMessage ?? "Order not found", correlationId);
        }
    }
}
