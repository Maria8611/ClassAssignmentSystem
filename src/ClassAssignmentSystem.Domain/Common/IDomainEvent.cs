using MediatR;

namespace ClassAssignmentSystem.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events are raised by aggregate roots when something meaningful happens in the domain.
/// They are dispatched AFTER the transaction commits — not during it.
/// MediatR's INotification makes them dispatchable via IPublisher.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the event occurred. Set at construction time, not dispatch time.
    /// </summary>
    DateTime OccurredOn { get; }
}