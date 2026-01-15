namespace OpenMind.Email.Contract;

/// <summary>
/// Marker interface for all Email domain events.
/// Used for SNS topic-based routing (fan-out pattern).
/// All Email events will be published to a single "email-events" SNS topic.
/// </summary>
public interface IEmailEvent { }

