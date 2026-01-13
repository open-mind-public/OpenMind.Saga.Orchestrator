using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.BuildingBlocks.Application.Behaviors;
using OpenMind.BuildingBlocks.Infrastructure.Behaviors;
using OpenMind.BuildingBlocks.Infrastructure.Persistence;
using OpenMind.Payment.Application.Commands.ProcessPayment;
using OpenMind.Payment.Domain.Repositories;
using OpenMind.Payment.Infrastructure.Consumers;
using OpenMind.Payment.Infrastructure.Repositories;
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

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessPaymentCommandConsumer>();
    x.AddConsumer<RefundPaymentCommandConsumer>();

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

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Payment" }))
.WithName("HealthCheck")
.WithOpenApi();

Log.Information("Payment Service starting...");
app.Run();
