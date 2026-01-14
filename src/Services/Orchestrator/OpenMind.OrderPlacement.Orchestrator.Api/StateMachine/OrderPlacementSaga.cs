using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using OpenMind.Email.Contract.Commands;
using OpenMind.Email.Contract.Events;
using OpenMind.Fulfillment.Contract.Commands;
using OpenMind.Fulfillment.Contract.Events;
using OpenMind.Order.Contract.Commands;
using OpenMind.Order.Contract.Events;
using OpenMind.Payment.Contract.Commands;
using OpenMind.Payment.Contract.Events;

namespace OpenMind.OrderPlacement.Orchestrator.Api.StateMachine;

/// <summary>
/// Order Placement Saga State Machine using MassTransit Automatonymous.
/// Implements the orchestrator pattern for coordinating the order placement workflow.
/// 
/// ASSUMPTION: Orders are created beforehand. The saga triggers the placement process.
/// 
/// Happy Path:
/// 1. Pending → Validating (Validate order exists in Order Service)
/// 2. Validating → PaymentProcessing (Process payment in Payment Service)
/// 3. PaymentProcessing → Fulfilling (Fulfill order in Fulfillment Service - async)
/// 4. Fulfilling → SendingConfirmation (Send confirmation email - async)
/// 5. SendingConfirmation → Completed
/// 
/// Error Paths:
/// - Validation Failed: Validating → Failed
/// - Payment Failed: PaymentProcessing → SendingPaymentFailedEmail → Cancelled
/// - Out of Stock: Fulfilling → RefundingPayment → SendingBackorderEmail → Cancelled
/// </summary>
public class OrderPlacementSaga : MassTransitStateMachine<OrderSagaState>
{
    private readonly ILogger<OrderPlacementSaga> _logger;

    // States
    public State Validating { get; private set; } = null!;
    public State PaymentProcessing { get; private set; } = null!;
    public State Fulfilling { get; private set; } = null!;
    public State SendingConfirmation { get; private set; } = null!;
    public State RefundingPayment { get; private set; } = null!;
    public State SendingPaymentFailedEmail { get; private set; } = null!;
    public State SendingBackorderEmail { get; private set; } = null!;
    public State SendingRefundEmail { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    // Events - Commands to start saga
    public Event<PlaceOrderCommand> PlaceOrder { get; private set; } = null!;

    // Events - Responses from Order Service
    public Event<OrderValidatedEvent> OrderValidated { get; private set; } = null!;
    public Event<OrderValidationFailedEvent> OrderValidationFailed { get; private set; } = null!;

    // Events - Responses from Payment Service
    public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; } = null!;
    public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = null!;
    public Event<PaymentRefundedEvent> PaymentRefunded { get; private set; } = null!;

    // Events - Responses from Fulfillment Service
    public Event<OrderShippedEvent> OrderShipped { get; private set; } = null!;
    public Event<FulfillmentFailedEvent> FulfillmentFailed { get; private set; } = null!;

    // Events - Responses from Email Service
    public Event<EmailSentEvent> EmailSent { get; private set; } = null!;
    public Event<EmailFailedEvent> EmailFailed { get; private set; } = null!;

    public OrderPlacementSaga(ILogger<OrderPlacementSaga> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        // Configure event correlation
        // PlaceOrderCommand creates saga with CorrelationId = OrderId
        Event(() => PlaceOrder, e => e.CorrelateById(context => context.Message.OrderId));

        // All response events correlate by OrderId (which matches the saga's CorrelationId)
        Event(() => OrderValidated, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });
        Event(() => OrderValidationFailed, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });

        Event(() => PaymentCompleted, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });
        Event(() => PaymentFailed, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });
        Event(() => PaymentRefunded, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });

        Event(() => OrderShipped, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });
        Event(() => FulfillmentFailed, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });

        Event(() => EmailSent, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });
        Event(() => EmailFailed, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.OnMissingInstance(m => m.Redeliver(r => r.Interval(5, TimeSpan.FromSeconds(2))));
        });

        // Define the state machine workflow
        Initially(
            When(PlaceOrder)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                    context.Saga.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "[Orchestrator] Received PlaceOrderCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Saga.CorrelationId);
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing ValidateOrderCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<ValidateOrderCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId
                    });
                })
                .TransitionTo(Validating));

        // Step 1: Order Validation
        During(Validating,
            When(OrderValidated)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Received OrderValidatedEvent - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.CorrelationId);

                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                    context.Saga.ShippingAddress = context.Message.ShippingAddress;
                    context.Saga.CustomerEmail = context.Message.CustomerEmail;
                    context.Saga.CustomerName = context.Message.CustomerName;
                    context.Saga.OrderItemsJson = JsonSerializer.Serialize(context.Message.Items);
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing ProcessPaymentCommand - OrderId: {OrderId}, Amount: {Amount}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.TotalAmount,
                        context.Saga.CorrelationId);

                    return context.Init<ProcessPaymentCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        CustomerId = context.Saga.CustomerId,
                        Amount = context.Saga.TotalAmount,
                        PaymentMethod = "CreditCard",
                        CardNumber = "4111111111111111",
                        CardExpiry = "12/26"
                    });
                })
                .TransitionTo(PaymentProcessing),

            When(OrderValidationFailed)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "[Orchestrator] Received OrderValidationFailedEvent - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.Reason,
                        context.Message.CorrelationId);

                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Failed));

        // Step 2: Payment Processing
        During(PaymentProcessing,
            When(PaymentCompleted)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Received PaymentCompletedEvent - OrderId: {OrderId}, PaymentId: {PaymentId}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.PaymentId,
                        context.Message.CorrelationId);

                    context.Saga.PaymentId = context.Message.PaymentId;
                    context.Saga.PaymentTransactionId = context.Message.TransactionId;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing MarkOrderAsPaymentCompletedCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<MarkOrderAsPaymentCompletedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        TransactionId = context.Message.TransactionId
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing FulfillOrderCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<FulfillOrderCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        CustomerId = context.Saga.CustomerId,
                        Items = JsonSerializer.Deserialize<List<FulfillmentItemDto>>(context.Saga.OrderItemsJson) ?? [],
                        ShippingAddress = context.Saga.ShippingAddress
                    });
                })
                .TransitionTo(Fulfilling),

            When(PaymentFailed)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "[Orchestrator] Received PaymentFailedEvent - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.Reason,
                        context.Message.CorrelationId);

                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.LastErrorCode = context.Message.ErrorCode;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing MarkOrderAsPaymentFailedCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<MarkOrderAsPaymentFailedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        Reason = context.Message.Reason
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing SendPaymentFailedEmailCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<SendPaymentFailedEmailCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        CustomerId = context.Saga.CustomerId,
                        CustomerEmail = context.Saga.CustomerEmail,
                        CustomerName = context.Saga.CustomerName,
                        FailureReason = context.Message.Reason
                    });
                })
                .TransitionTo(SendingPaymentFailedEmail));

        // Step 3: Fulfillment (Async)
        During(Fulfilling,
            When(OrderShipped)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Received OrderShippedEvent - OrderId: {OrderId}, TrackingNumber: {TrackingNumber}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.TrackingNumber,
                        context.Message.CorrelationId);

                    context.Saga.FulfillmentId = context.Message.FulfillmentId;
                    context.Saga.TrackingNumber = context.Message.TrackingNumber;
                    context.Saga.EstimatedDelivery = context.Message.EstimatedDelivery;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing MarkOrderAsShippedCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<MarkOrderAsShippedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        TrackingNumber = context.Message.TrackingNumber
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing SendOrderConfirmationEmailCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<SendOrderConfirmationEmailCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        CustomerId = context.Saga.CustomerId,
                        CustomerEmail = context.Saga.CustomerEmail,
                        CustomerName = context.Saga.CustomerName,
                        TotalAmount = context.Saga.TotalAmount,
                        TrackingNumber = context.Saga.TrackingNumber ?? string.Empty
                    });
                })
                .TransitionTo(SendingConfirmation),

            When(FulfillmentFailed)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "[Orchestrator] Received FulfillmentFailedEvent - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.Reason,
                        context.Message.CorrelationId);

                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing MarkOrderAsBackOrderedCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<MarkOrderAsBackOrderedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        Reason = context.Message.Reason
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing RefundPaymentCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<RefundPaymentCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        PaymentId = context.Saga.PaymentId ?? Guid.Empty,
                        Amount = context.Saga.TotalAmount,
                        Reason = "Items out of stock"
                    });
                })
                .TransitionTo(RefundingPayment));

        // Refunding Payment (Compensation)
        During(RefundingPayment,
            When(PaymentRefunded)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Received PaymentRefundedEvent - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing SendBackorderEmailCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<SendBackorderEmailCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        CustomerId = context.Saga.CustomerId,
                        CustomerEmail = context.Saga.CustomerEmail,
                        CustomerName = context.Saga.CustomerName,
                        BackorderedProducts = new List<string> { "Items from your order" },
                        EstimatedAvailability = DateTime.UtcNow.AddDays(14)
                    });
                })
                .TransitionTo(SendingBackorderEmail));

        // Sending Payment Failed Email
        During(SendingPaymentFailedEmail,
            When(EmailSent)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Received EmailSentEvent (PaymentFailed) - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled),

            When(EmailFailed)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "[Orchestrator] Received EmailFailedEvent (PaymentFailed) - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.Reason,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled)
        );

        // Sending Backorder Email
        During(SendingBackorderEmail,
            When(EmailSent)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Received EmailSentEvent (Backorder) - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Publishing SendRefundEmailCommand - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                        context.Saga.OrderId,
                        context.Saga.CorrelationId);

                    return context.Init<SendRefundEmailCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        CustomerId = context.Saga.CustomerId,
                        CustomerEmail = context.Saga.CustomerEmail,
                        CustomerName = context.Saga.CustomerName,
                        RefundAmount = context.Saga.TotalAmount
                    });
                })
                .TransitionTo(SendingRefundEmail),

            When(EmailFailed)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "[Orchestrator] Received EmailFailedEvent (Backorder) - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
                        context.Message.OrderId,
                        context.Message.Reason,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled));

        // Sending Refund Email
        During(SendingRefundEmail,
            When(EmailSent)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Received EmailSentEvent (Refund) - OrderId: {OrderId}, CorrelationId: {CorrelationId}. Saga completed with Cancelled state.",
                        context.Message.OrderId,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled),

            When(EmailFailed)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "[Orchestrator] Received EmailFailedEvent (Refund) - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}. Saga completed with Cancelled state.",
                        context.Message.OrderId,
                        context.Message.Reason,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled));

        // Step 4: Sending Confirmation Email (Final step for happy path)
        During(SendingConfirmation,
            When(EmailSent)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[Orchestrator] Received EmailSentEvent (Confirmation) - OrderId: {OrderId}, CorrelationId: {CorrelationId}. Saga completed successfully!",
                        context.Message.OrderId,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Completed),

            When(EmailFailed)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "[Orchestrator] Received EmailFailedEvent (Confirmation) - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}. Saga completed (email failed but order processed).",
                        context.Message.OrderId,
                        context.Message.Reason,
                        context.Message.CorrelationId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Completed));

        // Final states
        SetCompletedWhenFinalized();
    }
}

public record FulfillmentItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
