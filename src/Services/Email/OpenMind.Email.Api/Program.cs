using MassTransit;
using OpenMind.Email.Api.Endpoints;
using OpenMind.Email.Api.Features.SendBackorderEmail;
using OpenMind.Email.Api.Features.SendOrderCancelledEmail;
using OpenMind.Email.Api.Features.SendOrderConfirmationEmail;
using OpenMind.Email.Api.Features.SendPaymentFailedEmail;
using OpenMind.Email.Api.Features.SendRefundEmail;
using OpenMind.Email.IntegrationEvents.Commands;
using OpenMind.Email.IntegrationEvents.Events;
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

// MassTransit with Amazon SQS/SNS (Fan-out pattern)
var awsServiceUrl = builder.Configuration["AWS:ServiceURL"] ?? "http://localhost:4566";
var awsRegion = builder.Configuration["AWS:Region"] ?? "us-east-1";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendOrderConfirmationEmailConsumer>();
    x.AddConsumer<SendPaymentFailedEmailConsumer>();
    x.AddConsumer<SendOrderCancelledEmailConsumer>();
    x.AddConsumer<SendBackorderEmailConsumer>();
    x.AddConsumer<SendRefundEmailConsumer>();

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host(awsRegion, h =>
        {
            h.AccessKey("test");
            h.SecretKey("test");
            h.Config(new Amazon.SQS.AmazonSQSConfig { ServiceURL = awsServiceUrl });
            h.Config(new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceConfig { ServiceURL = awsServiceUrl });
        });

        // Configure fan-out pattern: all email commands → email-commands topic
        cfg.Message<SendOrderConfirmationEmailCommand>(m => m.SetEntityName("email-commands"));
        cfg.Message<SendPaymentFailedEmailCommand>(m => m.SetEntityName("email-commands"));
        cfg.Message<SendOrderCancelledEmailCommand>(m => m.SetEntityName("email-commands"));
        cfg.Message<SendBackorderEmailCommand>(m => m.SetEntityName("email-commands"));
        cfg.Message<SendRefundEmailCommand>(m => m.SetEntityName("email-commands"));

        // Configure fan-out pattern: all email events → email-events topic
        cfg.Message<EmailSentEvent>(m => m.SetEntityName("email-events"));
        cfg.Message<EmailFailedEvent>(m => m.SetEntityName("email-events"));

        // SQS queue for email-commands topic
        cfg.ReceiveEndpoint("email-service-commands", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Subscribe("email-commands", _ => { });
            e.ConfigureConsumer<SendOrderConfirmationEmailConsumer>(context);
            e.ConfigureConsumer<SendPaymentFailedEmailConsumer>(context);
            e.ConfigureConsumer<SendOrderCancelledEmailConsumer>(context);
            e.ConfigureConsumer<SendBackorderEmailConsumer>(context);
            e.ConfigureConsumer<SendRefundEmailConsumer>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthEndpoints("Email");

Log.Information("Email Service starting...");
app.Run();
