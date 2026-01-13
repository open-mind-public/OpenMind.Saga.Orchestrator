using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Email;
using OpenMind.Email.Application.Commands.SendEmail;
using OpenMind.Email.Domain.Enums;

namespace OpenMind.Email.Infrastructure.Consumers;

public class SendOrderConfirmationEmailConsumer : IConsumer<SendOrderConfirmationEmailCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendOrderConfirmationEmailConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<SendOrderConfirmationEmailCommand> context)
    {
        var command = new SendEmailCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            CustomerEmail = context.Message.CustomerEmail,
            CustomerName = context.Message.CustomerName,
            EmailType = EmailType.OrderConfirmation.Name,
            TemplateData = new Dictionary<string, string>
            {
                ["OrderId"] = context.Message.OrderId.ToString(),
                ["TrackingNumber"] = context.Message.TrackingNumber,
                ["TotalAmount"] = context.Message.TotalAmount.ToString("F2")
            }
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            await _publishEndpoint.Publish(new EmailSentEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                EmailType = EmailType.OrderConfirmation.Name,
                RecipientEmail = context.Message.CustomerEmail
            });
        }
        else
        {
            await _publishEndpoint.Publish(new EmailFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                EmailType = EmailType.OrderConfirmation.Name,
                Reason = result.ErrorMessage ?? "Unknown error"
            });
        }
    }
}
