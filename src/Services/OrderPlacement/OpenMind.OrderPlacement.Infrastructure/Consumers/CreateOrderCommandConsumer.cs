using MassTransit;
using MediatR;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using AppCommand = OpenMind.OrderPlacement.Application.Commands.CreateOrder;

namespace OpenMind.OrderPlacement.Infrastructure.Consumers;

/// <summary>
/// Consumer for CreateOrderCommand from the orchestrator.
/// </summary>
public class CreateOrderCommandConsumer : IConsumer<CreateOrderCommand>
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandConsumer(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<CreateOrderCommand> context)
    {
        var command = new AppCommand.CreateOrderCommand
        {
            OrderId = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            Items = context.Message.Items.Select(i => new AppCommand.OrderItemCommand
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            Street = context.Message.ShippingAddress,
            City = "Default City",
            State = "Default State",
            ZipCode = "00000",
            Country = "USA"
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                CustomerId = context.Message.CustomerId,
                TotalAmount = context.Message.TotalAmount
            });
        }
        else
        {
            await _publishEndpoint.Publish(new OrderCreationFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                Reason = result.ErrorMessage ?? "Unknown error"
            });
        }
    }
}
