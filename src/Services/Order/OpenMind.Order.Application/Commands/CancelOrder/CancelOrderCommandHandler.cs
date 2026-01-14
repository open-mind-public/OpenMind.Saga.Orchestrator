using Microsoft.Extensions.Logging;
using OpenMind.Order.Domain.Repositories;
using OpenMind.Shared.Application.Commands;

namespace OpenMind.Order.Application.Commands.CancelOrder;

public class CancelOrderCommandHandler(IOrderRepository orderRepository, ILogger<CancelOrderCommandHandler> logger)
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
            logger.LogError(ex, "[CancelOrder] ERROR: {Message}", ex.Message);
            return CommandResult.Failure(ex.Message, "CANCEL_ORDER_FAILED");
        }
    }
}
