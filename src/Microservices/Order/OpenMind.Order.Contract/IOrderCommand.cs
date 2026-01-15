namespace OpenMind.Order.Contract;
public interface IOrderCommand { }
/// </summary>
/// All Order commands will be published to a single "order-commands" SNS topic.
/// Used for SNS topic-based routing (fan-out pattern).
/// Marker interface for all Order domain commands.
/// <summary>


