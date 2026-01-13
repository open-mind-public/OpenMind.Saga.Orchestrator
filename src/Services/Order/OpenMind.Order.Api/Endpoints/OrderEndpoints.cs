using MediatR;
using OpenMind.Order.Application.Commands.CreateOrder;
using OpenMind.Order.Application.Queries.GetOrder;

namespace OpenMind.Order.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetOrder)
            .WithName("GetOrder");

        group.MapPost("", CreateOrder)
            .WithName("CreateOrder");

        return app;
    }

    private static async Task<IResult> GetOrder(Guid id, IMediator mediator)
    {
        var query = new GetOrderQuery(id);
        var result = await mediator.Send(query);
        return result.IsSuccess ? Results.Ok(result.Data) : Results.NotFound(result.ErrorMessage);
    }

    private static async Task<IResult> CreateOrder(CreateOrderRequest request, IMediator mediator)
    {
        var command = new CreateOrderCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Items = request.Items.Select(i => new OrderItemCommand
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            Street = request.ShippingAddress.Street,
            City = request.ShippingAddress.City,
            State = request.ShippingAddress.State,
            ZipCode = request.ShippingAddress.ZipCode,
            Country = request.ShippingAddress.Country
        };

        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Created($"/api/orders/{result.Data}", new { OrderId = result.Data })
            : Results.BadRequest(result.ErrorMessage);
    }
}

public record CreateOrderRequest(
    Guid CustomerId,
    List<OrderItemRequest> Items,
    AddressRequest ShippingAddress);

public record OrderItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public record AddressRequest(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);
