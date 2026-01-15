namespace OpenMind.Fulfillment.Contract;

/// <summary>
/// Marker interface for all Fulfillment domain events.
/// Used for SNS topic-based routing (fan-out pattern).
/// All Fulfillment events will be published to a single "fulfillment-events" SNS topic.
/// </summary>
public interface IFulfillmentEvent { }

