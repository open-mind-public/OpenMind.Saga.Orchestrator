using MassTransit;
using MongoDB.Driver;
using OpenMind.BuildingBlocks.Infrastructure.Persistence;
using OpenMind.Orchestrator.Api.StateMachine;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB
MongoDbConventions.Initialize();
var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>()
    ?? new MongoDbSettings { DatabaseName = "OrchestratorDb" };

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// MassTransit with Saga State Machine
builder.Services.AddMassTransit(x =>
{
    // Register the saga state machine
    x.AddSagaStateMachine<OrderPlacementSaga, OrderSagaState>()
        .MongoDbRepository(r =>
        {
            r.Connection = mongoSettings.ConnectionString;
            r.DatabaseName = mongoSettings.DatabaseName;
            r.CollectionName = "order_sagas";
        });

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// API Endpoints
app.MapPost("/api/orders/{orderId:guid}/place", async (Guid orderId, IPublishEndpoint publishEndpoint) =>
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
})
.WithName("PlaceOrder")
.WithOpenApi();

app.MapGet("/api/orders/{orderId:guid}/status", async (Guid orderId, IMongoDatabase database) =>
{
    var collection = database.GetCollection<OrderSagaState>("order_sagas");
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
})
.WithName("GetOrderStatus")
.WithOpenApi();

app.MapGet("/api/orders", async (IMongoDatabase database, int page = 1, int pageSize = 10) =>
{
    var collection = database.GetCollection<OrderSagaState>("order_sagas");
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
})
.WithName("GetOrders")
.WithOpenApi();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Orchestrator" }))
.WithName("HealthCheck")
.WithOpenApi();

Log.Information("Order Placement Orchestrator starting...");
app.Run();
