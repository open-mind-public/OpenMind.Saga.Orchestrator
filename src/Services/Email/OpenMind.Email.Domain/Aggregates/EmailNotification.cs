using OpenMind.Email.Domain.Enums;
using OpenMind.Email.Domain.Events;
using OpenMind.Email.Domain.Rules;
using OpenMind.Shared.Domain;

namespace OpenMind.Email.Domain.Aggregates;

public class EmailNotification : AggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string RecipientEmail { get; private set; }
    public string RecipientName { get; private set; }
    public EmailType Type { get; private set; }
    public EmailStatus Status { get; private set; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? SentAt { get; private set; }

    private EmailNotification() : base()
    {
        RecipientEmail = string.Empty;
        RecipientName = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
        Type = EmailType.OrderConfirmation;
        Status = EmailStatus.Pending;
    }

    private EmailNotification(
        Guid id,
        Guid orderId,
        Guid customerId,
        string recipientEmail,
        string recipientName,
        EmailType type,
        string subject,
        string body) : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        RecipientEmail = recipientEmail;
        RecipientName = recipientName;
        Type = type;
        Subject = subject;
        Body = body;
        Status = EmailStatus.Pending;
    }

    public static EmailNotification Create(
        Guid orderId,
        Guid customerId,
        string recipientEmail,
        string recipientName,
        EmailType type,
        string subject,
        string body)
    {
        CheckRule(new EmailRecipientMustBeValidRule(recipientEmail));
        CheckRule(new EmailSubjectMustBeProvidedRule(subject));
        CheckRule(new EmailBodyMustBeProvidedRule(body));

        return new EmailNotification(
            Guid.NewGuid(),
            orderId,
            customerId,
            recipientEmail,
            recipientName,
            type,
            subject,
            body);
    }

    public void MarkAsSent(Guid correlationId)
    {
        Status = EmailStatus.Sent;
        SentAt = DateTime.UtcNow;
        SetUpdatedAt();
        Emit(new EmailSentDomainEvent(Id, OrderId, Type.Name, RecipientEmail, correlationId));
    }

    public void MarkAsFailed(string reason, Guid correlationId)
    {
        Status = EmailStatus.Failed;
        FailureReason = reason;
        SetUpdatedAt();
        Emit(new EmailFailedDomainEvent(Id, OrderId, Type.Name, reason, correlationId));
    }
}
