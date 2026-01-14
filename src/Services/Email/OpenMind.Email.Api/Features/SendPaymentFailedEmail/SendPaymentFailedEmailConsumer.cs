using MassTransit;
using OpenMind.Email.IntegrationEvents.Commands;

namespace OpenMind.Email.Api.Features.SendPaymentFailedEmail;

public class SendPaymentFailedEmailConsumer(ILogger<SendPaymentFailedEmailConsumer> logger)
    : IConsumer<SendPaymentFailedEmailCommand>
{
    public Task Consume(ConsumeContext<SendPaymentFailedEmailCommand> context)
    {
        var message = context.Message;
        
        var subject = "Payment Issue with Your Order";
        var body = $"""
            Dear {message.CustomerName},

            We were unable to process your payment for your recent order.

            Order ID: {message.OrderId}
            Reason: {message.FailureReason}

            Please update your payment information and try again.

            Best regards,
            The OpenMind Team
            """;

        logger.LogInformation(
            "Sending payment failed email to {Email} for Order {OrderId}. Subject: {Subject}",
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

