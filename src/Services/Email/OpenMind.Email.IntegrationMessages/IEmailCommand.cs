namespace OpenMind.Email.IntegrationEvents;

/// <summary>
/// Marker interface for all Email domain commands.
/// Used for SNS topic-based routing (fan-out pattern).
/// All Email commands will be published to a single "email-commands" SNS topic.
/// </summary>
public interface IEmailCommand { }

