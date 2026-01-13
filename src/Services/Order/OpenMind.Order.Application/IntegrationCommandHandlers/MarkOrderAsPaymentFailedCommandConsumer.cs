using MassTransit;
using MediatR;
using OpenMind.Order.IntegrationEvents.Commands;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsPaymentFailed;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

public class MarkOrderAsPaymentFailedCommandConsumer(IMediator mediator)
    : IConsumer<MarkOrderAsPaymentFailedCommand>
{
    public async Task Consume(ConsumeContext<MarkOrderAsPaymentFailedCommand> context)
    {
        var command = new AppCommand.MarkOrderAsPaymentFailedCommand
        {
            OrderId = context.Message.OrderId,
            Reason = context.Message.Reason,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);
    }
}
