using MassTransit;
using OpenMind.Email.Contract.Commands;
using OpenMind.Email.Contract.Events;

namespace OpenMind.Email.Api.Features.SendBackorderEmail;

public class SendBackorderEmailConsumer(ILogger<SendBackorderEmailConsumer> logger)
    : IConsumer<SendBackorderEmailCommand>
{
    public async Task Consume(ConsumeContext<SendBackorderEmailCommand> context)
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

            await context.Publish(new EmailSentEvent
            {
                OrderId = message.OrderId,
                EmailType = "Backorder",
                RecipientEmail = message.CustomerEmail,
                CorrelationId = message.CorrelationId
            });
        }
        else
        {
            var reason = "SMTP server temporarily unavailable";
            logger.LogWarning(
                "Failed to send email to {Email} for Order {OrderId}: {Reason}",
                message.CustomerEmail,
                message.OrderId,
                reason);

            await context.Publish(new EmailFailedEvent
            {
                OrderId = message.OrderId,
                EmailType = "Backorder",
                Reason = reason,
                CorrelationId = message.CorrelationId
            });
        }
    }
}

