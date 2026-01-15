using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Order.Contract.Commands;
using AppCommand = OpenMind.Order.Application.Commands.MarkOrderAsPaymentFailed;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

public class MarkOrderAsPaymentFailedCommandConsumer(IMediator mediator, ILogger<MarkOrderAsPaymentFailedCommandConsumer> logger)
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

        logger.LogInformation("[Order] Consumed MarkOrderAsPaymentFailedCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Message.CorrelationId);
    }
}
