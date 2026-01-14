using MassTransit;
using OpenMind.Email.IntegrationEvents.Commands;
using OpenMind.Email.IntegrationEvents.Events;

namespace OpenMind.Email.Api.Features.SendOrderConfirmationEmail;

public class SendOrderConfirmationEmailConsumer(ILogger<SendOrderConfirmationEmailConsumer> logger)
    : IConsumer<SendOrderConfirmationEmailCommand>
{
    public async Task Consume(ConsumeContext<SendOrderConfirmationEmailCommand> context)
    {
        var message = context.Message;
        
        var subject = "Your Order Has Been Confirmed!";
        var body = $"""
            Dear {message.CustomerName},

            Thank you for your order. Your order has been confirmed and is being processed.

            Order ID: {message.OrderId}
            Tracking Number: {message.TrackingNumber}
            Total Amount: ${message.TotalAmount:F2}

            Best regards,
            The OpenMind Team
            """;

        // Simulate sending email (fire and forget)
        logger.LogInformation(
            "Sending order confirmation email to {Email} for Order {OrderId}. Subject: {Subject}",
            message.CustomerEmail,
            message.OrderId,
            subject);

        // Simulate 98% success rate
        if (Random.Shared.Next(100) < 98)
        {
            logger.LogInformation(
                "Email sent successfully to {Email} for Order {OrderId}",
                message.CustomerEmail,
                message.OrderId);

            await context.Publish(new EmailSentEvent
            {
                OrderId = message.OrderId,
                EmailType = "OrderConfirmation",
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
                EmailType = "OrderConfirmation",
                Reason = reason,
                CorrelationId = message.CorrelationId
            });
        }
    }
}

