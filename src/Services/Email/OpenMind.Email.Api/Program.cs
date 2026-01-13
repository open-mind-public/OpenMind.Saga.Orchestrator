using FluentValidation;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using OpenMind.BuildingBlocks.Application.Behaviors;
using OpenMind.BuildingBlocks.Infrastructure.Persistence;
using OpenMind.Email.Application.Commands.SendEmail;
using OpenMind.Email.Domain.Repositories;
using OpenMind.Email.Infrastructure.Consumers;
using OpenMind.Email.Infrastructure.Repositories;
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
    ?? new MongoDbSettings { DatabaseName = "EmailDb" };

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// Repositories
builder.Services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(SendEmailCommandHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(SendEmailCommandHandler).Assembly);

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendOrderConfirmationEmailConsumer>();
    x.AddConsumer<SendPaymentFailedEmailConsumer>();
    x.AddConsumer<SendOrderCancelledEmailConsumer>();
    x.AddConsumer<SendBackorderEmailConsumer>();
    x.AddConsumer<SendRefundEmailConsumer>();

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

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Email" }))
.WithName("HealthCheck")
.WithOpenApi();

Log.Information("Email Service starting...");
app.Run();
