using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.BuildingBlocks.Domain;
using OpenMind.Email.Domain.Aggregates;
using OpenMind.Email.Domain.Enums;
using OpenMind.Email.Domain.Repositories;

namespace OpenMind.Email.Application.Commands.SendEmail;

public record SendEmailCommand : ICommand<Guid>
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string EmailType { get; init; } = string.Empty;
    public Dictionary<string, string> TemplateData { get; init; } = [];
    public Guid CorrelationId { get; init; }
}

public class SendEmailCommandHandler(IEmailNotificationRepository repository)
    : ICommandHandler<SendEmailCommand, Guid>
{
    public async Task<CommandResult<Guid>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailType = Enumeration.FromDisplayName<EmailType>(request.EmailType);
            var (subject, body) = GenerateEmailContent(emailType, request.CustomerName, request.TemplateData);

            var email = EmailNotification.Create(
                request.OrderId,
                request.CustomerId,
                request.CustomerEmail,
                request.CustomerName,
                emailType,
                subject,
                body);

            // Simulate email sending (98% success rate)
            var sendSuccess = Random.Shared.Next(100) < 98;

            if (sendSuccess)
            {
                email.MarkAsSent(request.CorrelationId);
            }
            else
            {
                email.MarkAsFailed("SMTP server temporarily unavailable", request.CorrelationId);
            }

            await repository.AddAsync(email, cancellationToken);

            return sendSuccess
                ? CommandResult<Guid>.Success(email.Id)
                : CommandResult<Guid>.Failure("Failed to send email", "EMAIL_SEND_FAILED");
        }
        catch (Exception ex)
        {
            return CommandResult<Guid>.Failure(ex.Message, "EMAIL_ERROR");
        }
    }

    private static (string Subject, string Body) GenerateEmailContent(
        EmailType type,
        string customerName,
        Dictionary<string, string> data)
    {
        return type.Name switch
        {
            nameof(EmailType.OrderConfirmation) => (
                "Your Order Has Been Confirmed!",
                $"Dear {customerName},\n\nThank you for your order. Your order has been confirmed and is being processed.\n\n" +
                $"Order ID: {data.GetValueOrDefault("OrderId", "N/A")}\n" +
                $"Tracking Number: {data.GetValueOrDefault("TrackingNumber", "N/A")}\n\n" +
                "Best regards,\nThe OpenMind Team"),

            nameof(EmailType.PaymentFailed) => (
                "Payment Issue with Your Order",
                $"Dear {customerName},\n\nWe were unable to process your payment for your recent order.\n\n" +
                $"Order ID: {data.GetValueOrDefault("OrderId", "N/A")}\n" +
                $"Reason: {data.GetValueOrDefault("Reason", "Payment declined")}\n\n" +
                "Please update your payment information and try again.\n\nBest regards,\nThe OpenMind Team"),

            nameof(EmailType.OrderCancelled) => (
                "Your Order Has Been Cancelled",
                $"Dear {customerName},\n\nYour order has been cancelled.\n\n" +
                $"Order ID: {data.GetValueOrDefault("OrderId", "N/A")}\n" +
                $"Reason: {data.GetValueOrDefault("Reason", "Customer requested")}\n\n" +
                "If you have any questions, please contact us.\n\nBest regards,\nThe OpenMind Team"),

            nameof(EmailType.BackorderNotification) => (
                "Item Backorder Notification",
                $"Dear {customerName},\n\nSome items in your order are currently out of stock.\n\n" +
                $"Order ID: {data.GetValueOrDefault("OrderId", "N/A")}\n" +
                $"Estimated Availability: {data.GetValueOrDefault("EstimatedAvailability", "TBD")}\n\n" +
                "We will notify you when your items are back in stock.\n\nBest regards,\nThe OpenMind Team"),

            nameof(EmailType.RefundConfirmation) => (
                "Refund Confirmation",
                $"Dear {customerName},\n\nYour refund has been processed.\n\n" +
                $"Order ID: {data.GetValueOrDefault("OrderId", "N/A")}\n" +
                $"Refund Amount: ${data.GetValueOrDefault("Amount", "0.00")}\n\n" +
                "The refund will appear in your account within 5-7 business days.\n\nBest regards,\nThe OpenMind Team"),

            _ => (
                "Order Update",
                $"Dear {customerName},\n\nThis is an update regarding your order.\n\nBest regards,\nThe OpenMind Team")
        };
    }
}
