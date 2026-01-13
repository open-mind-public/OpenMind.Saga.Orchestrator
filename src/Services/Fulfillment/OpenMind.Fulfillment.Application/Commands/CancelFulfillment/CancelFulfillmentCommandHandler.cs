using OpenMind.Fulfillment.Domain.Repositories;
using OpenMind.Shared.Application.Commands;

namespace OpenMind.Fulfillment.Application.Commands.CancelFulfillment;

public class CancelFulfillmentCommandHandler(IFulfillmentRepository fulfillmentRepository)
    : ICommandHandler<CancelFulfillmentCommand>
{
    public async Task<CommandResult> Handle(CancelFulfillmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var fulfillment = await fulfillmentRepository.GetByIdAsync(request.FulfillmentId, cancellationToken)
                ?? await fulfillmentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (fulfillment is null)
                return CommandResult.Failure($"Fulfillment not found for order {request.OrderId}", "FULFILLMENT_NOT_FOUND");

            fulfillment.Cancel(request.CorrelationId);
            await fulfillmentRepository.UpdateAsync(fulfillment, cancellationToken);

            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message, "CANCEL_FULFILLMENT_ERROR");
        }
    }
}
