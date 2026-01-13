using MassTransit;
using MediatR;
using OpenMind.Email.IntegrationEvents.Commands;
using OpenMind.Email.Application.Commands.SendEmail;
using OpenMind.Email.Domain.Enums;

namespace OpenMind.Email.Application.IntegrationCommandHandlers;

public class SendOrderCancelledEmailConsumer(IMediator mediator)
    : IConsumer<SendOrderCancelledEmailCommand>
{
    public async Task Consume(ConsumeContext<SendOrderCancelledEmailCommand> context)
    {
        var command = new SendEmailCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            CustomerEmail = context.Message.CustomerEmail,
            CustomerName = context.Message.CustomerName,
            EmailType = EmailType.OrderCancelled.Name,
            CorrelationId = context.Message.CorrelationId,
            TemplateData = new Dictionary<string, string>
            {
                ["OrderId"] = context.Message.OrderId.ToString(),
                ["Reason"] = context.Message.CancellationReason
            }
        };

        await mediator.Send(command);
    }
}
