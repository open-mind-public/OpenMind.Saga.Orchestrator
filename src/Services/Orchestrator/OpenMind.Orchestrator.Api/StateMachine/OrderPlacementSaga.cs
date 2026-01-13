using MassTransit;
using OpenMind.Email.IntegrationEvents.Commands;
using OpenMind.Email.IntegrationEvents.Events;
using OpenMind.Fulfillment.IntegrationEvents;
using OpenMind.Fulfillment.IntegrationEvents.Commands;
using OpenMind.Fulfillment.IntegrationEvents.Events;
using OpenMind.Order.IntegrationEvents.Commands;
using OpenMind.Order.IntegrationEvents.Events;
using OpenMind.Payment.IntegrationEvents.Commands;
using OpenMind.Payment.IntegrationEvents.Events;
using System.Text.Json;

namespace OpenMind.Orchestrator.Api.StateMachine;

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

    public OrderPlacementSaga()
    {
        InstanceState(x => x.CurrentState);

        // Configure event correlation
        Event(() => PlaceOrder, e => e.CorrelateById(context => context.Message.OrderId));

        Event(() => OrderValidated, e => e.CorrelateById(context => context.Message.CorrelationId));
        Event(() => OrderValidationFailed, e => e.CorrelateById(context => context.Message.CorrelationId));

        Event(() => PaymentCompleted, e => e.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentFailed, e => e.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentRefunded, e => e.CorrelateById(context => context.Message.CorrelationId));

        Event(() => OrderShipped, e => e.CorrelateById(context => context.Message.CorrelationId));
        Event(() => FulfillmentFailed, e => e.CorrelateById(context => context.Message.CorrelationId));

        Event(() => EmailSent, e => e.CorrelateById(context => context.Message.CorrelationId));
        Event(() => EmailFailed, e => e.CorrelateById(context => context.Message.CorrelationId));

        // Define the state machine workflow
        Initially(
            When(PlaceOrder)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                // Validate the order exists and retrieve its details
                .PublishAsync(context => context.Init<ValidateOrderCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId
                }))
                .TransitionTo(Validating));

        // Step 1: Order Validation
        During(Validating,
            When(OrderValidated)
                .Then(context =>
                {
                    // Store order details from the validation response
                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                    context.Saga.ShippingAddress = context.Message.ShippingAddress;
                    context.Saga.CustomerEmail = context.Message.CustomerEmail;
                    context.Saga.CustomerName = context.Message.CustomerName;
                    context.Saga.OrderItemsJson = JsonSerializer.Serialize(context.Message.Items);
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context => context.Init<ProcessPaymentCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    CustomerId = context.Saga.CustomerId,
                    Amount = context.Saga.TotalAmount,
                    PaymentMethod = "CreditCard",
                    CardNumber = "4111111111111111", // Demo card
                    CardExpiry = "12/26"
                }))
                .TransitionTo(PaymentProcessing),

            When(OrderValidationFailed)
                .Then(context =>
                {
                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Failed));

        // Step 2: Payment Processing
        During(PaymentProcessing,
            When(PaymentCompleted)
                .Then(context =>
                {
                    context.Saga.PaymentId = context.Message.PaymentId;
                    context.Saga.PaymentTransactionId = context.Message.TransactionId;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                // Update order status then fulfill (async)
                .PublishAsync(context => context.Init<MarkOrderAsPaymentCompletedCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    TransactionId = context.Message.TransactionId
                }))
                .PublishAsync(context => context.Init<FulfillOrderCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    CustomerId = context.Saga.CustomerId,
                    Items = JsonSerializer.Deserialize<List<FulfillmentItemDto>>(context.Saga.OrderItemsJson) ?? [],
                    ShippingAddress = context.Saga.ShippingAddress
                }))
                .TransitionTo(Fulfilling),

            When(PaymentFailed)
                .Then(context =>
                {
                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.LastErrorCode = context.Message.ErrorCode;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                // Update order status to PaymentFailed
                .PublishAsync(context => context.Init<MarkOrderAsPaymentFailedCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Reason = context.Message.Reason
                }))
                // Send payment failed email (async)
                .PublishAsync(context => context.Init<SendPaymentFailedEmailCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    CustomerId = context.Saga.CustomerId,
                    CustomerEmail = context.Saga.CustomerEmail,
                    CustomerName = context.Saga.CustomerName,
                    FailureReason = context.Message.Reason
                }))
                .TransitionTo(SendingPaymentFailedEmail));

        // Step 3: Fulfillment (Async)
        During(Fulfilling,
            When(OrderShipped)
                .Then(context =>
                {
                    context.Saga.FulfillmentId = context.Message.FulfillmentId;
                    context.Saga.TrackingNumber = context.Message.TrackingNumber;
                    context.Saga.EstimatedDelivery = context.Message.EstimatedDelivery;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                // Update order status and send confirmation email (async)
                .PublishAsync(context => context.Init<MarkOrderAsShippedCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    TrackingNumber = context.Message.TrackingNumber
                }))
                .PublishAsync(context => context.Init<SendOrderConfirmationEmailCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    CustomerId = context.Saga.CustomerId,
                    CustomerEmail = context.Saga.CustomerEmail,
                    CustomerName = context.Saga.CustomerName,
                    TotalAmount = context.Saga.TotalAmount,
                    TrackingNumber = context.Saga.TrackingNumber ?? string.Empty
                }))
                .TransitionTo(SendingConfirmation),

            When(FulfillmentFailed)
                .Then(context =>
                {
                    context.Saga.LastError = context.Message.Reason;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                // Update order status to BackOrdered
                .PublishAsync(context => context.Init<MarkOrderAsBackOrderedCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Reason = context.Message.Reason
                }))
                // Refund payment due to out of stock
                .PublishAsync(context => context.Init<RefundPaymentCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    PaymentId = context.Saga.PaymentId ?? Guid.Empty,
                    Amount = context.Saga.TotalAmount,
                    Reason = "Items out of stock"
                }))
                .TransitionTo(RefundingPayment));

        // Refunding Payment (Compensation)
        During(RefundingPayment,
            When(PaymentRefunded)
                .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
                // Send backorder/refund notification
                .PublishAsync(context => context.Init<SendBackorderEmailCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    CustomerId = context.Saga.CustomerId,
                    CustomerEmail = context.Saga.CustomerEmail,
                    CustomerName = context.Saga.CustomerName,
                    BackorderedProducts = new List<string> { "Items from your order" },
                    EstimatedAvailability = DateTime.UtcNow.AddDays(14)
                }))
                .TransitionTo(SendingBackorderEmail));

        // Sending Payment Failed Email
        During(SendingPaymentFailedEmail,
            When(EmailSent)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled),

            When(EmailFailed)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled) // Continue even if email fails
        );

        // Sending Backorder Email
        During(SendingBackorderEmail,
            When(EmailSent)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                // Send refund confirmation email as well
                .PublishAsync(context => context.Init<SendRefundEmailCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    CustomerId = context.Saga.CustomerId,
                    CustomerEmail = context.Saga.CustomerEmail,
                    CustomerName = context.Saga.CustomerName,
                    RefundAmount = context.Saga.TotalAmount
                }))
                .TransitionTo(SendingRefundEmail),

            When(EmailFailed)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled));

        // Sending Refund Email
        During(SendingRefundEmail,
            When(EmailSent)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled),

            When(EmailFailed)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled));

        // Step 4: Sending Confirmation Email (Final step for happy path)
        During(SendingConfirmation,
            When(EmailSent)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Completed),

            When(EmailFailed)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                    // Log but don't fail the saga for email failures
                })
                .TransitionTo(Completed));

        // Final states
        SetCompletedWhenFinalized();
    }
}

/// <summary>
/// Command to initiate the order placement saga.
/// The order must already exist in the Order Service.
/// </summary>
public record PlaceOrderCommand
{
    /// <summary>
    /// The ID of an existing order to be placed.
    /// </summary>
    public Guid OrderId { get; init; }
}

public record FulfillmentItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
