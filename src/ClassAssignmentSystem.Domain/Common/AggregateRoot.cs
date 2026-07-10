namespace ClassAssignmentSystem.Domain.Common;

/// <summary>
/// Base class for aggregate roots.
///
/// An aggregate root is the ONLY entry point into an aggregate.
/// External objects may only hold a reference to the root — never to inner entities.
///
/// In this project the aggregate roots are:
///   Course      → owns Enrollments
///   Assignment  → references CourseId (not Course navigation)
///   Submission  → owns Grade (value object)
///   Student     → standalone
///   Teacher     → standalone
///
/// Repositories operate on aggregate roots only — there is no SubmissionLineItemRepository,
/// only a SubmissionRepository. This keeps transaction boundaries predictable.
///
/// Currently this class adds no extra members beyond BaseEntity — it is a semantic marker.
/// In a more advanced implementation you could add versioning / concurrency tokens here.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
}