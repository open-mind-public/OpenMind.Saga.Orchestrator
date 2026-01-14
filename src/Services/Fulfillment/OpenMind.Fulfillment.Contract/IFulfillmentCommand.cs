namespace OpenMind.Fulfillment.Contract;

/// <summary>
/// Marker interface for all Fulfillment domain commands.
/// Used for SNS topic-based routing (fan-out pattern).
/// All Fulfillment commands will be published to a single "fulfillment-commands" SNS topic.
/// </summary>
public interface IFulfillmentCommand { }

