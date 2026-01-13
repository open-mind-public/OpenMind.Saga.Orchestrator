using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Email;
using OpenMind.Email.Application.Commands.SendEmail;
using OpenMind.Email.Domain.Enums;

namespace OpenMind.Email.Infrastructure.Consumers;

public class SendBackorderEmailConsumer : IConsumer<SendBackorderEmailCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendBackorderEmailConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<SendBackorderEmailCommand> context)
    {
        var command = new SendEmailCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            CustomerEmail = context.Message.CustomerEmail,
            CustomerName = context.Message.CustomerName,
            EmailType = EmailType.BackorderNotification.Name,
            TemplateData = new Dictionary<string, string>
            {
                ["OrderId"] = context.Message.OrderId.ToString(),
                ["BackorderedProducts"] = string.Join(", ", context.Message.BackorderedProducts),
                ["EstimatedAvailability"] = context.Message.EstimatedAvailability.ToString("d")
            }
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            await _publishEndpoint.Publish(new EmailSentEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                EmailType = EmailType.BackorderNotification.Name,
                RecipientEmail = context.Message.CustomerEmail
            });
        }
    }
}
