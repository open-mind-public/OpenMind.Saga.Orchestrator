using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.Payment.Api.Endpoints;
using OpenMind.Payment.Application.Commands.ProcessPayment;
using OpenMind.Payment.Application.IntegrationCommandHandlers;
using OpenMind.Payment.Domain.Repositories;
using OpenMind.Payment.Infrastructure.Repositories;
using OpenMind.Payment.IntegrationEvents.Commands;
using OpenMind.Payment.IntegrationEvents.Events;
using OpenMind.Shared.Application.Behaviors;
using OpenMind.Shared.MongoDb;
using Serilog;
using ProcessPaymentCommand = OpenMind.Payment.IntegrationEvents.Commands.ProcessPaymentCommand;

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
    ?? new MongoDbSettings { DatabaseName = "PaymentDb" };

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// MongoDbContext - handles domain event dispatching
builder.Services.AddScoped<MongoDbContext>();

// Repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessPaymentCommandHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(ProcessPaymentCommandHandler).Assembly);

// MassTransit with Amazon SQS/SNS (Fan-out pattern)
var awsServiceUrl = builder.Configuration["AWS:ServiceURL"] ?? "http://localhost:4566";
var awsRegion = builder.Configuration["AWS:Region"] ?? "us-east-1";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessPaymentCommandConsumer>();
    x.AddConsumer<RefundPaymentCommandConsumer>();

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host(awsRegion, h =>
        {
            h.AccessKey("test");
            h.SecretKey("test");
            h.Config(new Amazon.SQS.AmazonSQSConfig { ServiceURL = awsServiceUrl });
            h.Config(new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceConfig { ServiceURL = awsServiceUrl });
        });

        // Configure fan-out pattern: all payment commands → payment-commands topic
        cfg.Message<ProcessPaymentCommand>(m => m.SetEntityName("payment-commands"));
        cfg.Message<RefundPaymentCommand>(m => m.SetEntityName("payment-commands"));

        // Configure fan-out pattern: all payment events → payment-events topic
        cfg.Message<PaymentCompletedEvent>(m => m.SetEntityName("payment-events"));
        cfg.Message<PaymentFailedEvent>(m => m.SetEntityName("payment-events"));
        cfg.Message<PaymentRefundedEvent>(m => m.SetEntityName("payment-events"));
        cfg.Message<PaymentRefundFailedEvent>(m => m.SetEntityName("payment-events"));

        // SQS queue for payment-commands topic
        cfg.ReceiveEndpoint("payment-service-commands", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("payment-commands", _ => { });
            e.ConfigureConsumer<ProcessPaymentCommandConsumer>(context);
            e.ConfigureConsumer<RefundPaymentCommandConsumer>(context);
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
app.MapHealthEndpoints("Payment");

Log.Information("Payment Service starting...");
app.Run();
