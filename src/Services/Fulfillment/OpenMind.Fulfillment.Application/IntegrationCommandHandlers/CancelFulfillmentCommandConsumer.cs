using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Fulfillment.IntegrationEvents.Commands;
using AppCommand = OpenMind.Fulfillment.Application.Commands.CancelFulfillment;

namespace OpenMind.Fulfillment.Application.IntegrationCommandHandlers;

public class CancelFulfillmentCommandConsumer(IMediator mediator, ILogger<CancelFulfillmentCommandConsumer> logger) 
    : IConsumer<CancelFulfillmentCommand>
{
    public async Task Consume(ConsumeContext<CancelFulfillmentCommand> context)
    {
        var command = new AppCommand.CancelFulfillmentCommand
        {
            OrderId = context.Message.OrderId,
            FulfillmentId = context.Message.FulfillmentId,
            Reason = context.Message.Reason,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);

        logger.LogInformation("[Fulfillment] Consumed CancelFulfillmentCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Message.CorrelationId);
    }
}
