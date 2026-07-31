using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClassAssignmentSystem.Infrastructure.Persistence.Repositories
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly AppDbContext _db;
        public SubmissionRepository(AppDbContext db) => _db = db;
        public async Task AddAsync(Submission submission, CancellationToken ct = default)
        {
            await _db.Submissions.AddAsync(submission, ct);
        }

        public void Delete(Submission submission)
        {
             _db.Submissions.Remove(submission);
        }

        public async Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken ct = default)
        {
            var res = await _db.Submissions
                        .Where(s => s.AssignmentId == assignmentId && s.StudentId == studentId)
                        .Include(s => s.Student)
                        .OrderByDescending(s => s.CreatedAt)
                        .FirstOrDefaultAsync(ct);
            return res;
        }

        public async Task<IReadOnlyList<Submission>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken ct = default)
        {
            var res = await _db.Submissions
                        .Where(s => s.AssignmentId == assignmentId)
                        .Include(s => s.Student)
                        .OrderByDescending(s => s.CreatedAt)
                        .ToListAsync(ct);
            return res;
        }

        public async Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var res = await _db.Submissions
                        .Where(s => s.Id == id)
                        .Include(s => s.Student)
                        .OrderByDescending(s => s.CreatedAt)
                        .FirstOrDefaultAsync(ct);
            return res;
        }

        public async Task<IReadOnlyList<Submission>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default)
        {
            var res = await _db.Submissions
                        .Where(s => s.StudentId == studentId)
                        .Include(s => s.Student)
                        .OrderByDescending(s => s.CreatedAt)
                        .ToListAsync(ct);
            return res;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
