using OpenMind.BuildingBlocks.Application.Commands;
using OpenMind.Payment.Domain.Repositories;

namespace OpenMind.Payment.Application.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;

    public ProcessPaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<CommandResult<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create payment
            var payment = Domain.Aggregates.Payment.Create(
                request.OrderId,
                request.CustomerId,
                request.Amount,
                request.PaymentMethod,
                request.CardNumber);

            payment.MarkAsProcessing();

            // Simulate payment processing with card validation
            var isValidCard = SimulatePaymentGateway(request.CardNumber, request.CardExpiry);

            if (isValidCard)
            {
                var transactionId = $"TXN-{Guid.NewGuid():N}".ToUpper()[..20];
                payment.MarkAsCompleted(transactionId);
            }
            else
            {
                payment.MarkAsFailed("Payment declined: Invalid or expired card");
            }

            await _paymentRepository.AddAsync(payment, cancellationToken);

            if (payment.Status == Domain.Enums.PaymentStatus.Completed)
            {
                return CommandResult<Guid>.Success(payment.Id);
            }

            return CommandResult<Guid>.Failure(payment.FailureReason ?? "Payment failed", "PAYMENT_DECLINED");
        }
        catch (Exception ex)
        {
            return CommandResult<Guid>.Failure(ex.Message, "PAYMENT_ERROR");
        }
    }

    private static bool SimulatePaymentGateway(string cardNumber, string expiry)
    {
        // Simulate card validation
        // Cards ending in 0000 are declined (for testing)
        if (cardNumber.EndsWith("0000"))
            return false;

        // Check expiry (simplified check)
        if (!string.IsNullOrEmpty(expiry))
        {
            var parts = expiry.Split('/');
            if (parts.Length == 2 && int.TryParse(parts[1], out var year))
            {
                var currentYear = DateTime.UtcNow.Year % 100;
                if (year < currentYear)
                    return false;
            }
        }

        // Simulate 90% success rate
        return Random.Shared.Next(100) < 90;
    }
}
