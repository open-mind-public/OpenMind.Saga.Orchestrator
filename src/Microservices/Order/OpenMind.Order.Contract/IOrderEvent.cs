namespace OpenMind.Order.Contract;
public interface IOrderEvent { }
/// </summary>
/// All Order events will be published to a single "order-events" SNS topic.
/// Used for SNS topic-based routing (fan-out pattern).
/// Marker interface for all Order domain events.
/// <summary>


