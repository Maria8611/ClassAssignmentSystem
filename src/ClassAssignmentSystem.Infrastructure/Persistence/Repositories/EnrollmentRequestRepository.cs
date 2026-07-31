using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClassAssignmentSystem.Infrastructure.Persistence.Repositories;
public class EnrollmentRequestRepository : IEnrollmentRequestRepository
{
    private readonly AppDbContext _db;
    public EnrollmentRequestRepository(AppDbContext db) => _db = db;
    public Task<EnrollmentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.EnrollmentRequests.Include(er => er.Course).Include(er => er.Student)
              .FirstOrDefaultAsync(er => er.Id == id, ct);
    public Task<EnrollmentRequest?> GetPendingByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        => _db.EnrollmentRequests.FirstOrDefaultAsync(er =>
            er.StudentId == studentId && er.CourseId == courseId && er.Status == EnrollmentStatus.Pending, ct);
    public Task<EnrollmentRequest?> GetApprovedByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
         => _db.EnrollmentRequests.FirstOrDefaultAsync(er =>
             er.StudentId == studentId && er.CourseId == courseId && er.Status == EnrollmentStatus.Approved, ct);

    public async Task<IReadOnlyList<EnrollmentRequest>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default)
        => await _db.EnrollmentRequests.Where(er => er.CourseId == courseId)
                    .OrderByDescending(er => er.CreatedAt).ToListAsync(ct);
    public async Task<IReadOnlyList<EnrollmentRequest>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default)
        => await _db.EnrollmentRequests.Where(er => er.StudentId == studentId)
                    .OrderByDescending(er => er.CreatedAt).ToListAsync(ct);
    public Task<int> GetApprovedCountByCourseAsync(Guid courseId, CancellationToken ct = default)
        => _db.EnrollmentRequests.CountAsync(er => er.CourseId == courseId && er.Status == EnrollmentStatus.Approved, ct);
    public async Task AddAsync(EnrollmentRequest request, CancellationToken ct = default)
        => await _db.EnrollmentRequests.AddAsync(request, ct);
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
