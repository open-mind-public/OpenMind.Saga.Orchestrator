using MassTransit;
using OpenMind.Email.IntegrationEvents.Commands;
using OpenMind.Email.IntegrationEvents.Events;

namespace OpenMind.Email.Api.Features.SendOrderCancelledEmail;

public class SendOrderCancelledEmailConsumer(ILogger<SendOrderCancelledEmailConsumer> logger)
    : IConsumer<SendOrderCancelledEmailCommand>
{
    public async Task Consume(ConsumeContext<SendOrderCancelledEmailCommand> context)
    {
        var message = context.Message;
        
        var subject = "Your Order Has Been Cancelled";
        var body = $"""
            Dear {message.CustomerName},

            Your order has been cancelled.

            Order ID: {message.OrderId}
            Reason: {message.CancellationReason}

            If you have any questions, please contact us.

            Best regards,
            The OpenMind Team
            """;

        logger.LogInformation(
            "Sending order cancelled email to {Email} for Order {OrderId}. Subject: {Subject}",
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
                EmailType = "OrderCancelled",
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
                EmailType = "OrderCancelled",
                Reason = reason,
                CorrelationId = message.CorrelationId
            });
        }
    }
}

