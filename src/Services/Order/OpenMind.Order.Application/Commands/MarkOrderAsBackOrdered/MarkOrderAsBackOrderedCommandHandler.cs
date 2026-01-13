using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Order.Domain.Repositories;

namespace OpenMind.Order.Application.Commands.MarkOrderAsBackOrdered;

public class MarkOrderAsBackOrderedCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<MarkOrderAsBackOrderedCommand>
{
    public async Task<CommandResult> Handle(MarkOrderAsBackOrderedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return CommandResult.Failure($"Order {request.OrderId} not found", "ORDER_NOT_FOUND");

            order.SetBackOrdered(request.Reason, request.CorrelationId);
            await orderRepository.UpdateAsync(order, cancellationToken);

            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message, "BACKORDERED_UPDATE_FAILED");
        }
    }
}
