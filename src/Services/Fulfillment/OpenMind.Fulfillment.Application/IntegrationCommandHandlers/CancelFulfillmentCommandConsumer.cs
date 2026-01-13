using MassTransit;
using MediatR;
using OpenMind.Fulfillment.IntegrationEvents.Commands;
using AppCommand = OpenMind.Fulfillment.Application.Commands.CancelFulfillment;

namespace OpenMind.Fulfillment.Application.IntegrationCommandHandlers;

public class CancelFulfillmentCommandConsumer(IMediator mediator) : IConsumer<CancelFulfillmentCommand>
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
    }
}
