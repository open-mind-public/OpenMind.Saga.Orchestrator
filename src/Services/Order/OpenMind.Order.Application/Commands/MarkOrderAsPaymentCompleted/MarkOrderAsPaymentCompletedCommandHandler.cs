using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Order.Domain.Repositories;

namespace OpenMind.Order.Application.Commands.MarkOrderAsPaymentCompleted;

public class MarkOrderAsPaymentCompletedCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<MarkOrderAsPaymentCompletedCommand>
{
    public async Task<CommandResult> Handle(MarkOrderAsPaymentCompletedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return CommandResult.Failure($"Order {request.OrderId} not found", "ORDER_NOT_FOUND");

            order.SetPaymentCompleted(request.TransactionId, request.CorrelationId);
            await orderRepository.UpdateAsync(order, cancellationToken);

            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message, "PAYMENT_COMPLETED_FAILED");
        }
    }
}
