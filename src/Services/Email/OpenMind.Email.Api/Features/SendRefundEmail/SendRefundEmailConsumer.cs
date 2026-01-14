using MassTransit;
using OpenMind.Email.IntegrationEvents.Commands;

namespace OpenMind.Email.Api.Features.SendRefundEmail;

public class SendRefundEmailConsumer(ILogger<SendRefundEmailConsumer> logger)
    : IConsumer<SendRefundEmailCommand>
{
    public Task Consume(ConsumeContext<SendRefundEmailCommand> context)
    {
        var message = context.Message;
        
        var subject = "Refund Confirmation";
        var body = $"""
            Dear {message.CustomerName},

            Your refund has been processed.

            Order ID: {message.OrderId}
            Refund Amount: ${message.RefundAmount:F2}

            The refund should appear in your account within 5-10 business days.

            Best regards,
            The OpenMind Team
            """;

        logger.LogInformation(
            "Sending refund confirmation email to {Email} for Order {OrderId}. Subject: {Subject}",
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

