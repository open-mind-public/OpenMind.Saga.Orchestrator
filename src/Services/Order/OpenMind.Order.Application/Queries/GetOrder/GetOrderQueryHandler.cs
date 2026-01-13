using OpenMind.BuildingBlocks.Application.Queries;
using OpenMind.Order.Domain.Repositories;

namespace OpenMind.Order.Application.Queries.GetOrder;

public class GetOrderQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<QueryResult<OrderDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return QueryResult<OrderDto>.NotFound($"Order {request.OrderId} not found");

        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId.Value,
            Status = order.Status.Name,
            TotalAmount = order.TotalAmount.Amount,
            ShippingAddress = order.ShippingAddress.ToString(),
            TrackingNumber = order.TrackingNumber,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.Amount,
                TotalPrice = item.TotalPrice.Amount
            }).ToList()
        };

        return QueryResult<OrderDto>.Success(orderDto);
    }
}
