
namespace ClassAssignmentSystem.Domain.Common;

/// <summary>
/// Base class for all domain entities.
///
/// Responsibilities:
///   - Provides a strongly-typed Id (Guid) — never int, never auto-increment from DB
///   - Tracks CreatedAt / UpdatedAt for auditing (set by AppDbContext, not manually)
///   - Holds domain events raised during a business operation (dispatched post-commit)
///
/// Why Guid?
///   Guids can be generated client-side before the DB round-trip, which matters in
///   event-sourcing and distributed scenarios. It also avoids leaking sequential IDs.
///
/// Why not record?
///   Entities have identity based on Id, not structural equality. Two Submission objects
///   with the same properties but different Ids are different entities. Records default
///   to structural equality — wrong semantic for entities.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }

    public DateTime UpdatedAt { get; protected set; }

    // ── Domain Events ────────────────────────────────────────────────────────
    // Private backing field — only the entity itself raises events.
    // Exposed as IReadOnlyCollection so infrastructure can read and clear them,
    // but nobody outside the entity can add to the list.

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Called by infrastructure (AppDbContext / UnitOfWork) after SaveChanges.
    /// Clears the events so they are not re-dispatched on the next save.
    /// </summary>
    public void ClearDomainEvents()
        => _domainEvents.Clear();

    // ── Equality ─────────────────────────────────────────────────────────────
    // Entities are equal when their Ids are equal — regardless of other properties.

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
        => !(left == right);
}