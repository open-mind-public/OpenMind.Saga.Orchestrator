using Microsoft.Extensions.Logging;
using OpenMind.Payment.Domain.Repositories;
using OpenMind.Shared.Application.Commands;

namespace OpenMind.Payment.Application.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler(IPaymentRepository paymentRepository, ILogger<ProcessPaymentCommandHandler> logger)
    : ICommandHandler<ProcessPaymentCommand, Guid>
{
    public async Task<CommandResult<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = Domain.Aggregates.Payment.Create(
                request.OrderId,
                request.CustomerId,
                request.Amount,
                request.PaymentMethod,
                request.CardNumber);

            payment.MarkAsProcessing(request.CardNumber, request.CardExpiry);

            await paymentRepository.AddAsync(payment, cancellationToken);

            return CommandResult<Guid>.Success(payment.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ProcessPayment] ERROR: {Message}", ex.Message);
            return CommandResult<Guid>.Failure(ex.Message, "PAYMENT_ERROR");
        }
    }
}
