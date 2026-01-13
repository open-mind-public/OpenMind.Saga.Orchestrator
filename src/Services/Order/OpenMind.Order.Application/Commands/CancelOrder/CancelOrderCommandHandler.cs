using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Order.Domain.Repositories;

namespace OpenMind.Order.Application.Commands.CancelOrder;

public class CancelOrderCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<CommandResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return CommandResult.Failure($"Order {request.OrderId} not found", "ORDER_NOT_FOUND");

            order.Cancel(request.Reason, request.CorrelationId);
            await orderRepository.UpdateAsync(order, cancellationToken);

            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message, "CANCEL_ORDER_FAILED");
        }
    }
}
