namespace OpenMind.Payment.Contract;

/// <summary>
/// Marker interface for all Payment domain events.
/// Used for SNS topic-based routing (fan-out pattern).
/// All Payment events will be published to a single "payment-events" SNS topic.
/// </summary>
public interface IPaymentEvent { }

