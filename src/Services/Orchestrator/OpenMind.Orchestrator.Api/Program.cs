using MassTransit;
using MongoDB.Driver;
using OpenMind.Orchestrator.Api.Endpoints;
using OpenMind.Orchestrator.Api.StateMachine;
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
    ?? new MongoDbSettings { DatabaseName = "OrchestratorDb" };

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// MassTransit with Saga State Machine
builder.Services.AddMassTransit(x =>
{
    // Register the saga state machine
    x.AddSagaStateMachine<OrderPlacementSaga, OrderSagaState>()
        .MongoDbRepository(r =>
        {
            r.Connection = mongoSettings.ConnectionString;
            r.DatabaseName = mongoSettings.DatabaseName;
            r.CollectionName = "order_sagas";
        });

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
app.MapOrderSagaEndpoints();
app.MapHealthEndpoints("Orchestrator");

Log.Information("Order Placement Orchestrator starting...");
app.Run();
