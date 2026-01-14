namespace OpenMind.Payment.Contract;

/// <summary>
/// Marker interface for all Payment domain commands.
/// Used for SNS topic-based routing (fan-out pattern).
/// All Payment commands will be published to a single "payment-commands" SNS topic.
/// </summary>
public interface IPaymentCommand { }

