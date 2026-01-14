using MassTransit;
using OpenMind.Email.IntegrationEvents.Commands;

namespace OpenMind.Email.Api.Features.SendBackorderEmail;

public class SendBackorderEmailConsumer(ILogger<SendBackorderEmailConsumer> logger)
    : IConsumer<SendBackorderEmailCommand>
{
    public Task Consume(ConsumeContext<SendBackorderEmailCommand> context)
    {
        var message = context.Message;
        
        var subject = "Item Backorder Notification";
        var body = $"""
            Dear {message.CustomerName},

            Some items in your order are currently out of stock.

            Order ID: {message.OrderId}
            Backordered Products: {string.Join(", ", message.BackorderedProducts)}
            Estimated Availability: {message.EstimatedAvailability:d}

            We will notify you when your items are back in stock.

            Best regards,
            The OpenMind Team
            """;

        logger.LogInformation(
            "Sending backorder notification email to {Email} for Order {OrderId}. Subject: {Subject}",
            message.CustomerEmail,
            message.OrderId,
            subject);

        if (Random.Shared.Next(100) < 98)
        {
            logger.LogInformation(
                "Email sent successfully to {Email} for Order {OrderId}",
                message.CustomerEmail,
                message.OrderId);
        }
        else
        {
            logger.LogWarning(
                "Failed to send email to {Email} for Order {OrderId}: SMTP server temporarily unavailable",
                message.CustomerEmail,
                message.OrderId);
        }

        return Task.CompletedTask;
    }
}

