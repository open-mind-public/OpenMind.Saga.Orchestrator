using MassTransit;
using OpenMind.Email.IntegrationEvents.Commands;

namespace OpenMind.Email.Api.Features.SendOrderCancelledEmail;

public class SendOrderCancelledEmailConsumer(ILogger<SendOrderCancelledEmailConsumer> logger)
    : IConsumer<SendOrderCancelledEmailCommand>
{
    public Task Consume(ConsumeContext<SendOrderCancelledEmailCommand> context)
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

