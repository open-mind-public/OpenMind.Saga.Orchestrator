using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.Order.Api.Endpoints;
using OpenMind.Order.Application.Commands.CreateOrder;
using OpenMind.Order.Application.IntegrationCommandHandlers;
using OpenMind.Order.Domain.Repositories;
using OpenMind.Order.Infrastructure.Repositories;
using OpenMind.Order.IntegrationEvents.Commands;
using OpenMind.Order.IntegrationEvents.Events;
using OpenMind.Shared.Application.Behaviors;
using OpenMind.Shared.MongoDb;
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

// MongoDbContext - handles domain event dispatching
builder.Services.AddScoped<MongoDbContext>();

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderCommandValidator).Assembly);

// MassTransit with Amazon SQS/SNS (Fan-out pattern)
var awsServiceUrl = builder.Configuration["AWS:ServiceURL"] ?? "http://localhost:4566";
var awsRegion = builder.Configuration["AWS:Region"] ?? "us-east-1";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ValidateOrderCommandConsumer>();
    x.AddConsumer<MarkOrderAsPaymentCompletedCommandConsumer>();
    x.AddConsumer<MarkOrderAsPaymentFailedCommandConsumer>();
    x.AddConsumer<MarkOrderAsShippedCommandConsumer>();
    x.AddConsumer<MarkOrderAsBackOrderedCommandConsumer>();
    x.AddConsumer<CancelOrderCommandConsumer>();

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host(awsRegion, h =>
        {
            h.AccessKey("test");
            h.SecretKey("test");
            h.Config(new Amazon.SQS.AmazonSQSConfig { ServiceURL = awsServiceUrl });
            h.Config(new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceConfig { ServiceURL = awsServiceUrl });
        });

        // Configure fan-out pattern: all order commands → order-commands topic
        cfg.Message<ValidateOrderCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<MarkOrderAsPaymentCompletedCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<MarkOrderAsPaymentFailedCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<MarkOrderAsShippedCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<MarkOrderAsBackOrderedCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<CancelOrderCommand>(m => m.SetEntityName("order-commands"));

        // Configure fan-out pattern: all order events → order-events topic
        cfg.Message<OrderValidatedEvent>(m => m.SetEntityName("order-events"));
        cfg.Message<OrderValidationFailedEvent>(m => m.SetEntityName("order-events"));
        cfg.Message<OrderPaymentCompletedEvent>(m => m.SetEntityName("order-events"));
        cfg.Message<OrderPaymentFailedEvent>(m => m.SetEntityName("order-events"));
        cfg.Message<OrderMarkedAsShippedEvent>(m => m.SetEntityName("order-events"));
        cfg.Message<OrderBackOrderedEvent>(m => m.SetEntityName("order-events"));
        cfg.Message<OrderCancelledEvent>(m => m.SetEntityName("order-events"));

        // SQS queue for order-commands topic
        cfg.ReceiveEndpoint("order-service-commands", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("order-commands", _ => { });
            e.ConfigureConsumer<ValidateOrderCommandConsumer>(context);
            e.ConfigureConsumer<MarkOrderAsPaymentCompletedCommandConsumer>(context);
            e.ConfigureConsumer<MarkOrderAsPaymentFailedCommandConsumer>(context);
            e.ConfigureConsumer<MarkOrderAsShippedCommandConsumer>(context);
            e.ConfigureConsumer<MarkOrderAsBackOrderedCommandConsumer>(context);
            e.ConfigureConsumer<CancelOrderCommandConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapOrderEndpoints();
app.MapHealthEndpoints("Order");

Log.Information("Order Service starting...");
app.Run();
