using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClassAssignmentSystem.Infrastructure.Persistence.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly AppDbContext _db;
        public AssignmentRepository(AppDbContext db) => _db = db;
        async Task IAssignmentRepository.AddAsync(Assignment assignment, CancellationToken cancellationToken)
                => await _db.Assignments.AddAsync(assignment, cancellationToken);


        async Task<IReadOnlyList<Assignment>> IAssignmentRepository.GetByCourseIdAsync(Guid courseId, CancellationToken ct)
            => await _db.Assignments.Where(a => a.CourseId == courseId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync(ct);

        async Task<Assignment?> IAssignmentRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        async Task<IReadOnlyList<Assignment>> IAssignmentRepository.GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken)
        => await _db.Assignments.Where(a => a.CreatedByTeacherId == teacherId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync(cancellationToken);

        async Task IAssignmentRepository.SaveChangesAsync(CancellationToken cancellationToken)
        => await _db.SaveChangesAsync(cancellationToken);
    }
}
