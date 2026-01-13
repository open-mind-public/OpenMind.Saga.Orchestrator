using OpenMind.Order.Domain.Repositories;
using OpenMind.Shared.Application.Commands;

namespace OpenMind.Order.Application.Commands.MarkOrderAsPaymentFailed;

public class MarkOrderAsPaymentFailedCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<MarkOrderAsPaymentFailedCommand>
{
    public async Task<CommandResult> Handle(MarkOrderAsPaymentFailedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return CommandResult.Failure($"Order {request.OrderId} not found", "ORDER_NOT_FOUND");

            order.SetPaymentFailed(request.Reason, request.CorrelationId);
            await orderRepository.UpdateAsync(order, cancellationToken);

            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message, "PAYMENT_FAILED_UPDATE_ERROR");
        }
    }
}
