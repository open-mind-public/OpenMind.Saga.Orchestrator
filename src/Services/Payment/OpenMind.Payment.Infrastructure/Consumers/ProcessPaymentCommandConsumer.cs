using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Payments;
using AppCommand = OpenMind.Payment.Application.Commands.ProcessPayment;

namespace OpenMind.Payment.Infrastructure.Consumers;

public class ProcessPaymentCommandConsumer : IConsumer<ProcessPaymentCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public ProcessPaymentCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
    {
        var command = new AppCommand.ProcessPaymentCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            Amount = context.Message.Amount,
            PaymentMethod = context.Message.PaymentMethod,
            CardNumber = context.Message.CardNumber,
            CardExpiry = context.Message.CardExpiry
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            await _publishEndpoint.Publish(new PaymentCompletedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                PaymentId = result.Data!,
                Amount = context.Message.Amount,
                TransactionId = $"TXN-{result.Data}"
            });
        }
        else
        {
            await _publishEndpoint.Publish(new PaymentFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                Reason = result.ErrorMessage ?? "Payment processing failed",
                ErrorCode = result.ErrorCode ?? "UNKNOWN"
            });
        }
    }
}
