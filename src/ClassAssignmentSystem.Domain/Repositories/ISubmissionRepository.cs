using ClassAssignmentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Domain.Repositories
{
    public interface ISubmissionRepository
    {
        Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken ct = default);
        Task<IReadOnlyList<Submission>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken ct = default);
        Task<IReadOnlyList<Submission>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default);
        Task AddAsync(Submission submission, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
