using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Payments;
using AppCommand = OpenMind.Payment.Application.Commands.RefundPayment;

namespace OpenMind.Payment.Infrastructure.Consumers;

public class RefundPaymentCommandConsumer : IConsumer<RefundPaymentCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public RefundPaymentCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<RefundPaymentCommand> context)
    {
        var command = new AppCommand.RefundPaymentCommand
        {
            OrderId = context.Message.OrderId,
            PaymentId = context.Message.PaymentId,
            Amount = context.Message.Amount,
            Reason = context.Message.Reason
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            await _publishEndpoint.Publish(new PaymentRefundedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                PaymentId = context.Message.PaymentId,
                Amount = context.Message.Amount
            });
        }
        else
        {
            await _publishEndpoint.Publish(new PaymentRefundFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                Reason = result.ErrorMessage ?? "Refund failed"
            });
        }
    }
}
