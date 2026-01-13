using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Fulfillment.Domain.Repositories;

namespace OpenMind.Fulfillment.Application.Commands.FulfillOrder;

public class FulfillOrderCommandHandler(IFulfillmentRepository fulfillmentRepository)
    : ICommandHandler<FulfillOrderCommand, FulfillOrderResult>
{
    public async Task<CommandResult<FulfillOrderResult>> Handle(FulfillOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var fulfillment = Domain.Aggregates.Fulfillment.Create(
                request.OrderId,
                request.CustomerId,
                request.ShippingAddress);

            foreach (var item in request.Items)
            {
                fulfillment.AddItem(item.ProductId, item.ProductName, item.Quantity);
            }

            fulfillment.MarkAsProcessing();

            // Simulate inventory check (85% success rate)
            var inventoryAvailable = SimulateInventoryCheck(request.Items);

            if (inventoryAvailable)
            {
                var trackingNumber = $"TRK-{Guid.NewGuid():N}".ToUpper()[..15];
                fulfillment.MarkAsShipped(trackingNumber, request.CorrelationId);

                await fulfillmentRepository.AddAsync(fulfillment, cancellationToken);

                return CommandResult<FulfillOrderResult>.Success(new FulfillOrderResult
                {
                    FulfillmentId = fulfillment.Id,
                    TrackingNumber = trackingNumber,
                    IsBackOrdered = false
                });
            }
            else
            {
                var backOrderedItems = request.Items
                    .Where(_ => Random.Shared.Next(100) < 50)
                    .Select(i => i.ProductName)
                    .ToList();

                if (backOrderedItems.Count == 0)
                    backOrderedItems.Add(request.Items.First().ProductName);

                fulfillment.MarkAsBackOrdered($"Items out of stock: {string.Join(", ", backOrderedItems)}", request.CorrelationId);

                await fulfillmentRepository.AddAsync(fulfillment, cancellationToken);

                return CommandResult<FulfillOrderResult>.Failure(
                    "Some items are out of stock",
                    "OUT_OF_STOCK");
            }
        }
        catch (Exception ex)
        {
            return CommandResult<FulfillOrderResult>.Failure(ex.Message, "FULFILLMENT_ERROR");
        }
    }

    private static bool SimulateInventoryCheck(List<FulfillmentItemCommand> items)
    {
        // 85% success rate for inventory availability
        return Random.Shared.Next(100) < 85;
    }
}
