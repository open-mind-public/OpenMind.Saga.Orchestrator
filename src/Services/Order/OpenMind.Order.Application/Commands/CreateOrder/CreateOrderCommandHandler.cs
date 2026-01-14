using Microsoft.Extensions.Logging;
using OpenMind.Order.Domain.Aggregates;
using OpenMind.Order.Domain.Entities;
using OpenMind.Order.Domain.Repositories;
using OpenMind.Order.Domain.ValueObjects;
using OpenMind.Shared.Application.Commands;

// Use alias to avoid namespace conflict
using OrderAggregate = OpenMind.Order.Domain.Aggregates.Order;

namespace OpenMind.Order.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler(IOrderRepository orderRepository, ILogger<CreateOrderCommandHandler> logger)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<CommandResult<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customerId = CustomerId.From(request.CustomerId);
            var shippingAddress = Address.Create(
                request.Street,
                request.City,
                request.State,
                request.ZipCode,
                request.Country);

            var order = OrderAggregate.Create(request.OrderId, customerId, shippingAddress);

            foreach (var item in request.Items)
            {
                var orderItem = OrderItem.Create(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    Money.Create(item.UnitPrice));

                order.AddItem(orderItem);
            }

            await orderRepository.AddAsync(order, cancellationToken);

            return CommandResult<Guid>.Success(order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CreateOrder] ERROR: {Message}", ex.Message);
            return CommandResult<Guid>.Failure(ex.Message, "CREATE_ORDER_FAILED");
        }
    }
}
