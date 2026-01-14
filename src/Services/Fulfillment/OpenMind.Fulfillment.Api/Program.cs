using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.Fulfillment.Api.Endpoints;
using OpenMind.Fulfillment.Application.Commands.FulfillOrder;
using OpenMind.Fulfillment.Application.IntegrationCommandHandlers;
using OpenMind.Fulfillment.Domain.Repositories;
using OpenMind.Fulfillment.Infrastructure.Repositories;
using OpenMind.Shared.Application.Behaviors;
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

// MongoDB
MongoDbConventions.Initialize();
var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>()
    ?? new MongoDbSettings { DatabaseName = "FulfillmentDb" };

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// Repositories
builder.Services.AddScoped<IFulfillmentRepository, FulfillmentRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(FulfillOrderCommandHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(FulfillOrderCommandHandler).Assembly);

// MassTransit with Amazon SQS/SNS
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

        cfg.ConfigureEndpoints(context);
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
