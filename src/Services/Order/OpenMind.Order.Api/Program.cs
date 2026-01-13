using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.Order.Api.Endpoints;
using OpenMind.Order.Application.Commands.CreateOrder;
using OpenMind.Order.Application.IntegrationCommandHandlers;
using OpenMind.Order.Domain.Repositories;
using OpenMind.Order.Infrastructure.Repositories;
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

// Map endpoints
app.MapOrderEndpoints();
app.MapHealthEndpoints("Order");

Log.Information("Order Service starting...");
app.Run();
