using MassTransit;
using MediatR;
using OpenMind.Fulfillment.IntegrationEvents.Commands;
using AppCommand = OpenMind.Fulfillment.Application.Commands.FulfillOrder;

namespace OpenMind.Fulfillment.Application.IntegrationCommandHandlers;

public class FulfillOrderCommandConsumer(IMediator mediator) : IConsumer<FulfillOrderCommand>
{
    public async Task Consume(ConsumeContext<FulfillOrderCommand> context)
    {
        var command = new AppCommand.FulfillOrderCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            Items = context.Message.Items.Select(i => new AppCommand.FulfillmentItemCommand(
                i.ProductId,
                i.ProductName,
                i.Quantity)).ToList(),
            ShippingAddress = context.Message.ShippingAddress,
            CorrelationId = context.Message.CorrelationId
        };

        await mediator.Send(command);
    }
}
