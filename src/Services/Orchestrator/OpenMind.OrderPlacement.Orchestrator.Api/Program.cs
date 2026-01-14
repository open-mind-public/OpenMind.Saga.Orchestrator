using MassTransit;
using MongoDB.Driver;
using OpenMind.Email.IntegrationEvents.Commands;
using OpenMind.Email.IntegrationEvents.Events;
using OpenMind.Fulfillment.IntegrationEvents.Commands;
using OpenMind.Fulfillment.IntegrationEvents.Events;
using OpenMind.Order.IntegrationEvents.Commands;
using OpenMind.Order.IntegrationEvents.Events;
using OpenMind.OrderPlacement.Orchestrator.Api.Endpoints;
using OpenMind.OrderPlacement.Orchestrator.Api.StateMachine;
using OpenMind.Payment.IntegrationEvents.Commands;
using OpenMind.Payment.IntegrationEvents.Events;
using OpenMind.Shared.MongoDb;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

MongoDbConventions.Initialize();
var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>()
    ?? new MongoDbSettings { DatabaseName = "OrderPlacementOrchestratorDb" };

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// MassTransit with Saga State Machine using Amazon SQS/SNS (Fan-out pattern)
var awsServiceUrl = builder.Configuration["AWS:ServiceURL"] ?? "http://localhost:4566";
var awsRegion = builder.Configuration["AWS:Region"] ?? "us-east-1";

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderPlacementSaga, OrderSagaState>()
        .MongoDbRepository(r =>
        {
            r.Connection = mongoSettings.ConnectionString;
            r.DatabaseName = mongoSettings.DatabaseName;
            r.CollectionName = "order_placement_sagas";
        });

    // Add delayed message scheduler for Redeliver support
    x.AddDelayedMessageScheduler();

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host(awsRegion, h =>
        {
            h.AccessKey("test");
            h.SecretKey("test");
            h.Config(new Amazon.SQS.AmazonSQSConfig { ServiceURL = awsServiceUrl });
            h.Config(new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceConfig { ServiceURL = awsServiceUrl });
        });

        // Use SQS delayed message scheduler for Redeliver functionality
        cfg.UseDelayedMessageScheduler();

        // Configure fan-out pattern: all order commands → order-commands topic
        cfg.Message<PlaceOrderCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<ValidateOrderCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<MarkOrderAsPaymentCompletedCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<MarkOrderAsPaymentFailedCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<MarkOrderAsShippedCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<MarkOrderAsBackOrderedCommand>(m => m.SetEntityName("order-commands"));
        cfg.Message<CancelOrderCommand>(m => m.SetEntityName("order-commands"));

        // Configure fan-out pattern: all order events → order-events topic
        cfg.Message<OrderValidatedEvent>(m => m.SetEntityName("order-events"));
        cfg.Message<OrderValidationFailedEvent>(m => m.SetEntityName("order-events"));

        // Configure fan-out pattern: all payment commands → payment-commands topic
        cfg.Message<ProcessPaymentCommand>(m => m.SetEntityName("payment-commands"));
        cfg.Message<RefundPaymentCommand>(m => m.SetEntityName("payment-commands"));

        // Configure fan-out pattern: all payment events → payment-events topic
        cfg.Message<PaymentCompletedEvent>(m => m.SetEntityName("payment-events"));
        cfg.Message<PaymentFailedEvent>(m => m.SetEntityName("payment-events"));
        cfg.Message<PaymentRefundedEvent>(m => m.SetEntityName("payment-events"));

        // Configure fan-out pattern: all fulfillment commands → fulfillment-commands topic
        cfg.Message<FulfillOrderCommand>(m => m.SetEntityName("fulfillment-commands"));
        cfg.Message<CancelFulfillmentCommand>(m => m.SetEntityName("fulfillment-commands"));

        // Configure fan-out pattern: all fulfillment events → fulfillment-events topic
        cfg.Message<OrderShippedEvent>(m => m.SetEntityName("fulfillment-events"));
        cfg.Message<FulfillmentFailedEvent>(m => m.SetEntityName("fulfillment-events"));

        // Configure fan-out pattern: all email commands → email-commands topic
        cfg.Message<SendOrderConfirmationEmailCommand>(m => m.SetEntityName("email-commands"));
        cfg.Message<SendPaymentFailedEmailCommand>(m => m.SetEntityName("email-commands"));
        cfg.Message<SendOrderCancelledEmailCommand>(m => m.SetEntityName("email-commands"));
        cfg.Message<SendBackorderEmailCommand>(m => m.SetEntityName("email-commands"));
        cfg.Message<SendRefundEmailCommand>(m => m.SetEntityName("email-commands"));

        // Configure fan-out pattern: all email events → email-events topic
        cfg.Message<EmailSentEvent>(m => m.SetEntityName("email-events"));
        cfg.Message<EmailFailedEvent>(m => m.SetEntityName("email-events"));

        // SQS queue for order-commands topic (Orchestrator receives PlaceOrderCommand to start saga)
        cfg.ReceiveEndpoint("orchestrator-order-commands", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("order-commands", _ => { });
            e.ConfigureSaga<OrderSagaState>(context);
        });

        // SQS queue for order-events topic (Orchestrator consumes order events)
        // UseMessageRetry handles race condition where event arrives before saga is persisted
        cfg.ReceiveEndpoint("orchestrator-order-events", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("order-events", _ => { });
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)));
            e.ConfigureSaga<OrderSagaState>(context);
        });

        // SQS queue for payment-events topic (Orchestrator consumes payment events)
        cfg.ReceiveEndpoint("orchestrator-payment-events", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("payment-events", _ => { });
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)));
            e.ConfigureSaga<OrderSagaState>(context);
        });

        // SQS queue for fulfillment-events topic (Orchestrator consumes fulfillment events)
        cfg.ReceiveEndpoint("orchestrator-fulfillment-events", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("fulfillment-events", _ => { });
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)));
            e.ConfigureSaga<OrderSagaState>(context);
        });

        // SQS queue for email-events topic (Orchestrator consumes email events)
        cfg.ReceiveEndpoint("orchestrator-email-events", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("email-events", _ => { });
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)));
            e.ConfigureSaga<OrderSagaState>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapOrderSagaEndpoints();
app.MapHealthEndpoints("OrderPlacementOrchestrator");

Log.Information("Order Placement Orchestrator starting...");
app.Run();
