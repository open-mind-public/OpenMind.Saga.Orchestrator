using MassTransit;
using MediatR;
using OpenMind.Email.IntegrationEvents.Commands;
using OpenMind.Email.Application.Commands.SendEmail;
using OpenMind.Email.Domain.Enums;

namespace OpenMind.Email.Application.IntegrationCommandHandlers;

public class SendRefundEmailConsumer(IMediator mediator)
    : IConsumer<SendRefundEmailCommand>
{
    public async Task Consume(ConsumeContext<SendRefundEmailCommand> context)
    {
        var command = new SendEmailCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            CustomerEmail = context.Message.CustomerEmail,
            CustomerName = context.Message.CustomerName,
            EmailType = EmailType.RefundConfirmation.Name,
            CorrelationId = context.Message.CorrelationId,
            TemplateData = new Dictionary<string, string>
            {
                ["OrderId"] = context.Message.OrderId.ToString(),
                ["Amount"] = context.Message.RefundAmount.ToString("F2")
            }
        };

        await mediator.Send(command);
    }
}
