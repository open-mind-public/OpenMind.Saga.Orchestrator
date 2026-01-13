using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.BuildingBlocks.Application.Behaviors;
using OpenMind.BuildingBlocks.Infrastructure.Persistence;
using OpenMind.Fulfillment.Application.Commands.FulfillOrder;
using OpenMind.Fulfillment.Domain.Repositories;
using OpenMind.Fulfillment.Infrastructure.Consumers;
using OpenMind.Fulfillment.Infrastructure.Repositories;
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

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FulfillOrderCommandConsumer>();
    x.AddConsumer<CancelFulfillmentCommandConsumer>();

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Fulfillment" }))
.WithName("HealthCheck")
.WithOpenApi();

Log.Information("Fulfillment Service starting...");
app.Run();
