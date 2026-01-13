using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsPaymentFailed;

namespace OpenMind.Order.Infrastructure.Consumers;

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
