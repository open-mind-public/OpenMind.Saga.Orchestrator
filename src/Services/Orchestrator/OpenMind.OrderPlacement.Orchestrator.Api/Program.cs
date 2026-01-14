using MassTransit;
using MongoDB.Driver;
using OpenMind.OrderPlacement.Orchestrator.Api.Endpoints;
using OpenMind.OrderPlacement.Orchestrator.Api.StateMachine;
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

// MassTransit with Saga State Machine using Amazon SQS/SNS
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

app.MapOrderSagaEndpoints();
app.MapHealthEndpoints("OrderPlacementOrchestrator");

Log.Information("Order Placement Orchestrator starting...");
app.Run();
