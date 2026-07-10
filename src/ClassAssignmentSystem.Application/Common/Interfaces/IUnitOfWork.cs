namespace ClassAssignmentSystem.Application.Common.Interfaces;

/// <summary>
/// Unit of Work — coordinates a single database transaction across multiple repositories.
///
/// Why do we need this alongside repositories?
///   Repositories stage changes (Add, Remove) but don't commit them.
///   UnitOfWork.SaveChangesAsync() flushes ALL staged changes in ONE transaction.
///
///   Example: CreateCourseCommand
///     1. courseRepository.Add(course)          ← staged, not persisted
///     2. await unitOfWork.SaveChangesAsync()   ← one INSERT, one transaction
///
///   Example: EnrollStudentCommand (touches Course and potentially raises events)
///     1. course.EnrollStudent(studentId)       ← domain logic, raises event
///     2. await unitOfWork.SaveChangesAsync()   ← persists + dispatches domain events
///
/// The command handler depends on IUnitOfWork — never on DbContext directly.
/// This keeps handlers testable: mock IUnitOfWork, verify SaveChangesAsync was called.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Flushes all pending changes to the database in a single transaction.
    /// Also dispatches any domain events raised during the operation.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}