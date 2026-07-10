using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClassAssignmentSystem.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken ct = default)
        => await _db.Users.Where(u => u.Role == role).ToListAsync(ct);
    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);
    public async Task AddAsync(User user, CancellationToken ct = default) => await _db.Users.AddAsync(user, ct);
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}



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
