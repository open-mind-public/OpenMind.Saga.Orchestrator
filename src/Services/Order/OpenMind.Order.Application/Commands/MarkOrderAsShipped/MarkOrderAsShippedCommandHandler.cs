using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Order.Domain.Repositories;

namespace OpenMind.Order.Application.Commands.MarkOrderAsShipped;

public class MarkOrderAsShippedCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<MarkOrderAsShippedCommand>
{
    public async Task<CommandResult> Handle(MarkOrderAsShippedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return CommandResult.Failure($"Order {request.OrderId} not found", "ORDER_NOT_FOUND");

            order.SetShipped(request.TrackingNumber, request.CorrelationId);
            await orderRepository.UpdateAsync(order, cancellationToken);

            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message, "SHIPPED_UPDATE_FAILED");
        }
    }
}
