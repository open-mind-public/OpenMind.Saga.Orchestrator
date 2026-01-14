using MassTransit;
using OpenMind.Email.Contract.Commands;
using OpenMind.Email.Contract.Events;

namespace OpenMind.Email.Api.Features.SendPaymentFailedEmail;

public class SendPaymentFailedEmailConsumer(ILogger<SendPaymentFailedEmailConsumer> logger)
    : IConsumer<SendPaymentFailedEmailCommand>
{
    public async Task Consume(ConsumeContext<SendPaymentFailedEmailCommand> context)
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

            await context.Publish(new EmailSentEvent
            {
                OrderId = message.OrderId,
                EmailType = "PaymentFailed",
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
                EmailType = "PaymentFailed",
                Reason = reason,
                CorrelationId = message.CorrelationId
            });
        }
    }
}

