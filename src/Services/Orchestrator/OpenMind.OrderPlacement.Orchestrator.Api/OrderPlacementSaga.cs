using System.Text.Json;
using MassTransit;
using OpenMind.Email.Contract.Commands;
using OpenMind.Email.Contract.Events;
using OpenMind.Fulfillment.Contract.Commands;
using OpenMind.Fulfillment.Contract.Events;
using OpenMind.Order.Contract.Commands;
using OpenMind.Order.Contract.Events;
using OpenMind.Payment.Contract.Commands;
using OpenMind.Payment.Contract.Events;

namespace OpenMind.OrderPlacement.Orchestrator.Api;

/// <summary>
/// Order Placement Saga State Machine using MassTransit Automatonymous.
/// Implements the orchestrator pattern for coordinating the order placement workflow.
/// 
/// Happy Path:
/// 1. Initial → Validating (Validate order exists)
/// 2. Validating → PaymentProcessing (Process payment)
/// 3. PaymentProcessing → Fulfilling (Fulfill order)
/// 4. Fulfilling → SendingConfirmation (Send confirmation email)
/// 5. SendingConfirmation → Completed
/// 
/// Error Paths (Retryable):
/// - Validation Failed: Validating → ValidationFailed (retry via PlaceOrder)
/// - Payment Not Paid: PaymentProcessing → PaymentNotPaid (retry via Payment API, then continues to Fulfilling)
/// 
/// Error Paths (Terminal):
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
    public State PaymentNotPaid { get; private set; } = null!;
    public State RefundingPayment { get; private set; } = null!;
    public State SendingBackorderEmail { get; private set; } = null!;
    public State SendingRefundEmail { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;
    
    // Failed states (retryable)
    public State ValidationFailed { get; private set; } = null!;

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

        Event(() => PlaceOrder, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.InsertOnInitial = true;
            e.SetSagaFactory(context => new OrderSagaState
            {
                CorrelationId = context.Message.OrderId
            });
        });

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

                    _logger.LogInformation("[Saga] PlaceOrder - OrderId: {OrderId}, CorrelationId: {CorrelationId}", context.Message.OrderId, context.Saga.CorrelationId);
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing ValidateOrderCommand - OrderId: {OrderId}", context.Saga.OrderId);

                    return context.Init<ValidateOrderCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId
                    });
                })
                .TransitionTo(Validating));

        // Allow retry from ValidationFailed state
        During(ValidationFailed,
            When(PlaceOrder)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] Retrying from ValidationFailed - OrderId: {OrderId}", context.Message.OrderId);
                    context.Saga.LastError = null;
                    context.Saga.RetryCount++;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context => context.Init<ValidateOrderCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId
                }))
                .TransitionTo(Validating));

        // Ignore retry for all other states
        DuringAny(
            When(PlaceOrder)
                .Then(context =>
                {
                    _logger.LogWarning("[Saga] PlaceOrder ignored (in progress) - OrderId: {OrderId}, State: {State}", context.Message.OrderId, context.Saga.CurrentState);
                }));

        // Step 1: Order Validation
        During(Validating,
            When(OrderValidated)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] OrderValidated - OrderId: {OrderId}", context.Message.OrderId);

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
                    _logger.LogInformation("[Saga] Publishing ProcessPaymentCommand - OrderId: {OrderId}, Amount: {Amount}", context.Saga.OrderId, context.Saga.TotalAmount);

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
                    _logger.LogWarning("[Saga] OrderValidationFailed - OrderId: {OrderId}, Reason: {Reason}", context.Message.OrderId, context.Message.Reason);

                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .TransitionTo(ValidationFailed));

        // Step 2: Payment Processing
        During(PaymentProcessing,
            When(PaymentCompleted)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] PaymentCompleted - OrderId: {OrderId}, PaymentId: {PaymentId}", context.Message.OrderId, context.Message.PaymentId);

                    context.Saga.PaymentId = context.Message.PaymentId;
                    context.Saga.PaymentTransactionId = context.Message.TransactionId;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing MarkOrderAsPaymentCompletedCommand - OrderId: {OrderId}", context.Saga.OrderId);

                    return context.Init<MarkOrderAsPaymentCompletedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        TransactionId = context.Message.TransactionId
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing FulfillOrderCommand - OrderId: {OrderId}", context.Saga.OrderId);

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
                    _logger.LogWarning("[Saga] PaymentFailed - OrderId: {OrderId}, Reason: {Reason}", context.Message.OrderId, context.Message.Reason);

                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.LastErrorCode = context.Message.ErrorCode;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing MarkOrderAsPaymentFailedCommand - OrderId: {OrderId}", context.Saga.OrderId);

                    return context.Init<MarkOrderAsPaymentFailedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        Reason = context.Message.Reason
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing SendPaymentFailedEmailCommand - OrderId: {OrderId}", context.Saga.OrderId);

                    return context.Init<SendPaymentFailedEmailCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        CustomerId = context.Saga.CustomerId,
                        CustomerEmail = context.Saga.CustomerEmail,
                        CustomerName = context.Saga.CustomerName,
                        Reason = context.Message.Reason
                    });
                })
                .TransitionTo(PaymentNotPaid));

        // PaymentNotPaid - awaiting payment retry via Payment API
        During(PaymentNotPaid,
            When(PaymentCompleted)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] PaymentCompleted (Retry) - OrderId: {OrderId}, PaymentId: {PaymentId}", context.Message.OrderId, context.Message.PaymentId);

                    context.Saga.PaymentId = context.Message.PaymentId;
                    context.Saga.PaymentTransactionId = context.Message.TransactionId;
                    context.Saga.LastError = null;
                    context.Saga.LastErrorCode = null;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing MarkOrderAsPaymentCompletedCommand - OrderId: {OrderId}", context.Saga.OrderId);

                    return context.Init<MarkOrderAsPaymentCompletedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        TransactionId = context.Message.TransactionId
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing FulfillOrderCommand - OrderId: {OrderId}", context.Saga.OrderId);

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

            When(EmailSent)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] EmailSent (PaymentFailed) - OrderId: {OrderId}. Awaiting payment retry.", context.Message.OrderId);
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                }),

            When(EmailFailed)
                .Then(context =>
                {
                    _logger.LogWarning("[Saga] EmailFailed (PaymentFailed) - OrderId: {OrderId}, Reason: {Reason}. Awaiting payment retry.", context.Message.OrderId, context.Message.Reason);
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
            );

        // Step 3: Fulfillment (Async)
        During(Fulfilling,
            When(OrderShipped)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] OrderShipped - OrderId: {OrderId}, TrackingNumber: {TrackingNumber}", context.Message.OrderId, context.Message.TrackingNumber);

                    context.Saga.FulfillmentId = context.Message.FulfillmentId;
                    context.Saga.TrackingNumber = context.Message.TrackingNumber;
                    context.Saga.EstimatedDelivery = context.Message.EstimatedDelivery;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing MarkOrderAsShippedCommand - OrderId: {OrderId}", context.Saga.OrderId);

                    return context.Init<MarkOrderAsShippedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        TrackingNumber = context.Message.TrackingNumber
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing SendOrderConfirmationEmailCommand - OrderId: {OrderId}", context.Saga.OrderId);

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
                    _logger.LogWarning("[Saga] FulfillmentFailed - OrderId: {OrderId}, Reason: {Reason}", context.Message.OrderId, context.Message.Reason);

                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing MarkOrderAsBackOrderedCommand - OrderId: {OrderId}", context.Saga.OrderId);

                    return context.Init<MarkOrderAsBackOrderedCommand>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        Reason = context.Message.Reason
                    });
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing RefundPaymentCommand - OrderId: {OrderId}", context.Saga.OrderId);

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
                    _logger.LogInformation("[Saga] PaymentRefunded - OrderId: {OrderId}", context.Message.OrderId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing SendBackorderEmailCommand - OrderId: {OrderId}", context.Saga.OrderId);

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

        // Sending Backorder Email
        During(SendingBackorderEmail,
            When(EmailSent)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] EmailSent (Backorder) - OrderId: {OrderId}", context.Message.OrderId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .PublishAsync(context =>
                {
                    _logger.LogInformation("[Saga] Publishing SendRefundEmailCommand - OrderId: {OrderId}", context.Saga.OrderId);

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
                    _logger.LogWarning("[Saga] EmailFailed (Backorder) - OrderId: {OrderId}, Reason: {Reason}", context.Message.OrderId, context.Message.Reason);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled));

        // Sending Refund Email
        During(SendingRefundEmail,
            When(EmailSent)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] EmailSent (Refund) - OrderId: {OrderId}. Saga cancelled.", context.Message.OrderId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled),

            When(EmailFailed)
                .Then(context =>
                {
                    _logger.LogWarning("[Saga] EmailFailed (Refund) - OrderId: {OrderId}, Reason: {Reason}. Saga cancelled.", context.Message.OrderId, context.Message.Reason);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled));

        // Step 4: Sending Confirmation Email (Final step for happy path)
        During(SendingConfirmation,
            When(EmailSent)
                .Then(context =>
                {
                    _logger.LogInformation("[Saga] EmailSent (Confirmation) - OrderId: {OrderId}. Saga completed!", context.Message.OrderId);

                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Completed),

            When(EmailFailed)
                .Then(context =>
                {
                    _logger.LogWarning("[Saga] EmailFailed (Confirmation) - OrderId: {OrderId}, Reason: {Reason}. Order processed.", context.Message.OrderId, context.Message.Reason);

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
