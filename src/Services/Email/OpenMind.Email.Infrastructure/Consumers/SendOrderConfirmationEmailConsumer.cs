using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Email;
using OpenMind.Email.Application.Commands.SendEmail;
using OpenMind.Email.Domain.Enums;

namespace OpenMind.Email.Infrastructure.Consumers;

public class SendOrderConfirmationEmailConsumer(IMediator mediator)
    : IConsumer<SendOrderConfirmationEmailCommand>
{
    public async Task Consume(ConsumeContext<SendOrderConfirmationEmailCommand> context)
    {
        var command = new SendEmailCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            CustomerEmail = context.Message.CustomerEmail,
            CustomerName = context.Message.CustomerName,
            EmailType = EmailType.OrderConfirmation.Name,
            CorrelationId = context.Message.CorrelationId,
            TemplateData = new Dictionary<string, string>
            {
                ["OrderId"] = context.Message.OrderId.ToString(),
                ["TrackingNumber"] = context.Message.TrackingNumber,
                ["TotalAmount"] = context.Message.TotalAmount.ToString("F2")
            }
        };

        await mediator.Send(command);
    }
}
