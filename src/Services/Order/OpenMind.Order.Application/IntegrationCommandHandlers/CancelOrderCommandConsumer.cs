using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Order.Contract.Commands;
using AppCommand = OpenMind.Order.Application.Commands.CancelOrder;

namespace OpenMind.Order.Application.IntegrationCommandHandlers;

/// <summary>
/// Consumer for CancelOrderCommand from the orchestrator.
/// </summary>
public class CancelOrderCommandConsumer(IMediator mediator, ILogger<CancelOrderCommandConsumer> logger) 
    : IConsumer<CancelOrderCommand>
{
    public async Task Consume(ConsumeContext<CancelOrderCommand> context)
    {
        var command = new AppCommand.CancelOrderCommand
        {
            OrderId = context.Message.OrderId,
            Reason = context.Message.Reason,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);

        logger.LogInformation("[Order] Consumed CancelOrderCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Message.CorrelationId);
    }
}
