using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Email;
using OpenMind.Email.Application.Commands.SendEmail;
using OpenMind.Email.Domain.Enums;

namespace OpenMind.Email.Infrastructure.Consumers;

public class SendBackorderEmailConsumer(IMediator mediator)
    : IConsumer<SendBackorderEmailCommand>
{
    public async Task Consume(ConsumeContext<SendBackorderEmailCommand> context)
    {
        var command = new SendEmailCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            CustomerEmail = context.Message.CustomerEmail,
            CustomerName = context.Message.CustomerName,
            EmailType = EmailType.BackorderNotification.Name,
            CorrelationId = context.Message.CorrelationId,
            TemplateData = new Dictionary<string, string>
            {
                ["OrderId"] = context.Message.OrderId.ToString(),
                ["BackorderedProducts"] = string.Join(", ", context.Message.BackorderedProducts),
                ["EstimatedAvailability"] = context.Message.EstimatedAvailability.ToString("d")
            }
        };

        await mediator.Send(command);
    }
}
