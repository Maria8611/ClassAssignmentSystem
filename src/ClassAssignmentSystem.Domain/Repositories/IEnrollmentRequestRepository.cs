using ClassAssignmentSystem.Domain.Entities;

public interface IEnrollmentRequestRepository
{
    Task<EnrollmentRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EnrollmentRequest?> GetPendingByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentRequest>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentRequest>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<int> GetApprovedCountByCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task AddAsync(EnrollmentRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}