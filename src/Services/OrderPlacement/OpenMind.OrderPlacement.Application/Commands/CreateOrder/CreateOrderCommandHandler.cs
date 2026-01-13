using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.OrderPlacement.Domain.Aggregates;
using OpenMind.OrderPlacement.Domain.Entities;
using OpenMind.OrderPlacement.Domain.Repositories;
using OpenMind.OrderPlacement.Domain.ValueObjects;

namespace OpenMind.OrderPlacement.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

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

            var order = Order.Create(request.OrderId, customerId, shippingAddress);

            foreach (var item in request.Items)
            {
                var orderItem = OrderItem.Create(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    Money.Create(item.UnitPrice));

                order.AddItem(orderItem);
            }

            await _orderRepository.AddAsync(order, cancellationToken);

            return CommandResult<Guid>.Success(order.Id);
        }
        catch (Exception ex)
        {
            return CommandResult<Guid>.Failure(ex.Message, "CREATE_ORDER_FAILED");
        }
    }
}
