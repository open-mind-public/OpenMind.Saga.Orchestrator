using MassTransit;
using MediatR;
using OpenMind.Order.IntegrationEvents.Commands;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsPaymentCompleted;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

public class MarkOrderAsPaymentCompletedCommandConsumer(IMediator mediator)
    : IConsumer<MarkOrderAsPaymentCompletedCommand>
{
    public async Task Consume(ConsumeContext<MarkOrderAsPaymentCompletedCommand> context)
    {
        var command = new AppCommand.MarkOrderAsPaymentCompletedCommand
        {
            OrderId = context.Message.OrderId,
            TransactionId = context.Message.TransactionId,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);
    }
}
