using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenMind.Payment.Contract.Commands;
using AppCommand = OpenMind.Payment.Application.Commands.RefundPayment;

namespace OpenMind.Payment.Application.IntegrationCommandHandlers;

public class RefundPaymentCommandConsumer(IMediator mediator, ILogger<RefundPaymentCommandConsumer> logger) 
    : IConsumer<RefundPaymentCommand>
{
    public async Task Consume(ConsumeContext<RefundPaymentCommand> context)
    {
        var command = new AppCommand.RefundPaymentCommand
        {
            OrderId = context.Message.OrderId,
            PaymentId = context.Message.PaymentId,
            Amount = context.Message.Amount,
            Reason = context.Message.Reason,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);

        logger.LogInformation("[Payment] Consumed RefundPaymentCommand - OrderId: {OrderId}, Amount: {Amount}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Message.Amount, context.Message.CorrelationId);
    }
}
