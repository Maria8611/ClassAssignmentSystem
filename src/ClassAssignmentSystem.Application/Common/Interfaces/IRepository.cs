using System.Linq.Expressions;
using ClassAssignmentSystem.Domain.Common;

namespace ClassAssignmentSystem.Application.Common.Interfaces;

/// <summary>
/// Generic repository contract for aggregate roots.
///
/// The Application layer depends on THIS interface — never on EF Core or DbContext directly.
/// Infrastructure implements it with EF Core. Tests implement it with in-memory fakes.
///
/// Scope: one repository per AGGREGATE ROOT only.
///   ✅ ICourseRepository     (Course is an aggregate root)
///   ✅ IAssignmentRepository (Assignment is an aggregate root)
///   ❌ IEnrollmentRepository (Enrollment is part of the Course aggregate — access via Course)
///
/// Why no Update() method?
///   EF Core tracks entity state automatically. Once you fetch an entity via GetByIdAsync,
///   mutate it, then call UnitOfWork.SaveChangesAsync() — EF detects and persists the change.
///   An explicit Update() method would be redundant and potentially confusing.
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
    /// <summary>
    /// Fetch by primary key. Returns null if not found.
    /// Use the specific repository's GetByIdAsync if you need eager-loaded navigation.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns ALL entities. Use with caution on large tables — prefer filtered overloads.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns entities matching the predicate.
    /// Keep predicates simple — complex queries belong in specific repository methods
    /// that can use .Include() and projection to avoid N+1.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if any entity matches the predicate.
    /// Translates to SQL EXISTS — more efficient than fetching and checking Count.
    /// </summary>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new entity for insertion.
    /// The INSERT does not hit the DB until UnitOfWork.SaveChangesAsync() is called.
    /// </summary>
    void Add(T entity);

    /// <summary>
    /// Stages an entity for deletion.
    /// The DELETE does not hit the DB until UnitOfWork.SaveChangesAsync() is called.
    /// </summary>
    void Remove(T entity);
}