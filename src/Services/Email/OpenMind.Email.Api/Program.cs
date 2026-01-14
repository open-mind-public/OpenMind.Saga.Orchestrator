using MassTransit;
using OpenMind.Email.Api.Endpoints;
using OpenMind.Email.Api.Features.SendBackorderEmail;
using OpenMind.Email.Api.Features.SendOrderCancelledEmail;
using OpenMind.Email.Api.Features.SendOrderConfirmationEmail;
using OpenMind.Email.Api.Features.SendPaymentFailedEmail;
using OpenMind.Email.Api.Features.SendRefundEmail;
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


// MassTransit with Amazon SQS/SNS
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

        cfg.ConfigureEndpoints(context);
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
