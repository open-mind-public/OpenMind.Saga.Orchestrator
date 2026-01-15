using MassTransit;
using MongoDB.Driver;
using OpenMind.Order.Contract.Commands;
using OpenMind.OrderPlacement.Orchestrator.Api.StateMachine;

namespace OpenMind.OrderPlacement.Orchestrator.Api.Endpoints;

public static class OrderSagaEndpoints
{
    public static IEndpointRouteBuilder MapOrderSagaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithOpenApi();

        group.MapPost("/{orderId:guid}/place", PlaceOrder)
            .WithName("PlaceOrder");

        group.MapGet("/{orderId:guid}/status", GetOrderStatus)
            .WithName("GetOrderStatus");

        group.MapGet("", GetOrders)
            .WithName("GetOrders");

        return app;
    }

    private static async Task<IResult> PlaceOrder(Guid orderId, IPublishEndpoint publishEndpoint)
    {
        // Publish command to start the saga
        // The order must already exist in the Order Service
        await publishEndpoint.Publish(new PlaceOrderCommand
        {
            OrderId = orderId
        });

        return Results.Accepted($"/api/orders/{orderId}/status", new
        {
            OrderId = orderId,
            Message = "Order placement initiated",
            Status = "Validating"
        });
    }

    private static async Task<IResult> GetOrderStatus(Guid orderId, IMongoDatabase database)
    {
        var collection = database.GetCollection<OrderSagaState>("order_placement_sagas");
        var filter = Builders<OrderSagaState>.Filter.Eq(x => x.OrderId, orderId);
        var saga = await collection.Find(filter).FirstOrDefaultAsync();

        if (saga is null)
            return Results.NotFound(new { Message = $"Order {orderId} not found" });

        return Results.Ok(new
        {
            saga.OrderId,
            saga.CustomerId,
            saga.CurrentState,
            saga.TotalAmount,
            saga.TrackingNumber,
            saga.EstimatedDelivery,
            saga.LastError,
            saga.CreatedAt,
            saga.UpdatedAt,
            saga.CompletedAt
        });
    }

    private static async Task<IResult> GetOrders(IMongoDatabase database, int page = 1, int pageSize = 10)
    {
        var collection = database.GetCollection<OrderSagaState>("order_placement_sagas");
        var totalCount = await collection.CountDocumentsAsync(_ => true);
        var sagas = await collection
            .Find(_ => true)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return Results.Ok(new
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Orders = sagas.Select(s => new
            {
                s.OrderId,
                s.CustomerId,
                s.CurrentState,
                s.TotalAmount,
                s.TrackingNumber,
                s.CreatedAt
            })
        });
    }
}
