using MassTransit;
using MediatR;
using OpenMind.Payment.IntegrationEvents.Commands;
using AppCommand = OpenMind.Payment.Application.Commands.RefundPayment;

namespace OpenMind.Payment.Application.IntegrationCommandHandlers;

public class RefundPaymentCommandConsumer(IMediator mediator) : IConsumer<RefundPaymentCommand>
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
    }
}
