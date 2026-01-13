using MassTransit;
using MediatR;
using OpenMind.Email.IntegrationEvents.Commands;
using OpenMind.Email.Application.Commands.SendEmail;
using OpenMind.Email.Domain.Enums;

namespace OpenMind.Email.Application.IntegrationCommandHandlers;

public class SendPaymentFailedEmailConsumer(IMediator mediator)
    : IConsumer<SendPaymentFailedEmailCommand>
{
    public async Task Consume(ConsumeContext<SendPaymentFailedEmailCommand> context)
    {
        var command = new SendEmailCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            CustomerEmail = context.Message.CustomerEmail,
            CustomerName = context.Message.CustomerName,
            EmailType = EmailType.PaymentFailed.Name,
            CorrelationId = context.Message.CorrelationId,
            TemplateData = new Dictionary<string, string>
            {
                ["OrderId"] = context.Message.OrderId.ToString(),
                ["Reason"] = context.Message.FailureReason
            }
        };

        await mediator.Send(command);
    }
}
