using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Fulfillment.Domain.Repositories;

namespace OpenMind.Fulfillment.Application.Commands.CancelFulfillment;

public class CancelFulfillmentCommandHandler : ICommandHandler<CancelFulfillmentCommand>
{
    private readonly IFulfillmentRepository _fulfillmentRepository;

    public CancelFulfillmentCommandHandler(IFulfillmentRepository fulfillmentRepository)
    {
        _fulfillmentRepository = fulfillmentRepository;
    }

    public async Task<CommandResult> Handle(CancelFulfillmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var fulfillment = await _fulfillmentRepository.GetByIdAsync(request.FulfillmentId, cancellationToken)
                ?? await _fulfillmentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (fulfillment is null)
                return CommandResult.Failure($"Fulfillment not found for order {request.OrderId}", "FULFILLMENT_NOT_FOUND");

            fulfillment.Cancel();
            await _fulfillmentRepository.UpdateAsync(fulfillment, cancellationToken);

            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message, "CANCEL_FULFILLMENT_ERROR");
        }
    }
}
