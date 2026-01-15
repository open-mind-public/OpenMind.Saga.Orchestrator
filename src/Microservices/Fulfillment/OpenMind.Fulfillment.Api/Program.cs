using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.Fulfillment.Api.Endpoints;
using OpenMind.Fulfillment.Application.Commands.FulfillOrder;
using OpenMind.Fulfillment.Application.IntegrationCommandHandlers;
using OpenMind.Fulfillment.Contract.Commands;
using OpenMind.Fulfillment.Contract.Events;
using OpenMind.Fulfillment.Domain.Repositories;
using OpenMind.Fulfillment.Infrastructure.Repositories;
using OpenMind.Shared.Application.Behaviors;
using OpenMind.Shared.MongoDb;
using Serilog;
using FulfillOrderCommand = OpenMind.Fulfillment.Contract.Commands.FulfillOrderCommand;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB
MongoDbConventions.Initialize();
var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>()
    ?? new MongoDbSettings { DatabaseName = "FulfillmentDb" };

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// MongoDbContext - handles domain event dispatching
builder.Services.AddScoped<MongoDbContext>();

// Repositories
builder.Services.AddScoped<IFulfillmentRepository, FulfillmentRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(FulfillOrderCommandHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(FulfillOrderCommandHandler).Assembly);

// MassTransit with Amazon SQS/SNS (Fan-out pattern)
var awsServiceUrl = builder.Configuration["AWS:ServiceURL"] ?? "http://localhost:4566";
var awsRegion = builder.Configuration["AWS:Region"] ?? "us-east-1";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FulfillOrderCommandConsumer>();
    x.AddConsumer<CancelFulfillmentCommandConsumer>();

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host(awsRegion, h =>
        {
            h.AccessKey("test");
            h.SecretKey("test");
            h.Config(new Amazon.SQS.AmazonSQSConfig { ServiceURL = awsServiceUrl });
            h.Config(new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceConfig { ServiceURL = awsServiceUrl });
        });

        // Configure fan-out pattern: all fulfillment commands → fulfillment-commands topic
        cfg.Message<FulfillOrderCommand>(m => m.SetEntityName("fulfillment-commands"));
        cfg.Message<CancelFulfillmentCommand>(m => m.SetEntityName("fulfillment-commands"));

        // Configure fan-out pattern: all fulfillment events → fulfillment-events topic
        cfg.Message<OrderShippedEvent>(m => m.SetEntityName("fulfillment-events"));
        cfg.Message<FulfillmentFailedEvent>(m => m.SetEntityName("fulfillment-events"));
        cfg.Message<FulfillmentCancelledEvent>(m => m.SetEntityName("fulfillment-events"));
        cfg.Message<FulfillmentInitiatedEvent>(m => m.SetEntityName("fulfillment-events"));
        cfg.Message<ItemBackorderedEvent>(m => m.SetEntityName("fulfillment-events"));

        // SQS queue for fulfillment-commands topic
        cfg.ReceiveEndpoint("fulfillment-service-commands", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("fulfillment-commands", _ => { });
            e.ConfigureConsumer<FulfillOrderCommandConsumer>(context);
            e.ConfigureConsumer<CancelFulfillmentCommandConsumer>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapHealthEndpoints("Fulfillment");

Log.Information("Fulfillment Service starting...");
app.Run();
