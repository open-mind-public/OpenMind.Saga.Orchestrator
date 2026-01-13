using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.BuildingBlocks.Application.Behaviors;
using OpenMind.BuildingBlocks.Infrastructure.Persistence;
using OpenMind.Order.Application.Commands.CreateOrder;
using OpenMind.Order.Domain.Repositories;
using OpenMind.Order.Infrastructure.Consumers;
using OpenMind.Order.Infrastructure.Repositories;
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
    ?? new MongoDbSettings { DatabaseName = "OrderPlacementDb" };

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderCommandValidator).Assembly);

// MassTransit with In-Memory Transport
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ValidateOrderCommandConsumer>();
    x.AddConsumer<MarkOrderAsPaymentCompletedCommandConsumer>();
    x.AddConsumer<MarkOrderAsPaymentFailedCommandConsumer>();
    x.AddConsumer<MarkOrderAsShippedCommandConsumer>();
    x.AddConsumer<MarkOrderAsBackOrderedCommandConsumer>();
    x.AddConsumer<CancelOrderCommandConsumer>();

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

// Minimal API Endpoints
app.MapGet("/api/orders/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var query = new OpenMind.Order.Application.Queries.GetOrder.GetOrderQuery(id);
    var result = await mediator.Send(query);
    return result.IsSuccess ? Results.Ok(result.Data) : Results.NotFound(result.ErrorMessage);
})
.WithName("GetOrder")
.WithOpenApi();

app.MapPost("/api/orders", async (CreateOrderRequest request, IMediator mediator) =>
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
})
.WithName("CreateOrder")
.WithOpenApi();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Order" }))
.WithName("HealthCheck")
.WithOpenApi();

Log.Information("Order Service starting...");
app.Run();

// Request DTOs
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
