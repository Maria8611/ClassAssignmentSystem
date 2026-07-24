using ClassAssignmentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Domain.Repositories
{
    public interface IAssignmentRepository
    {
        Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Assignment>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default);
        Task<IReadOnlyList<Assignment>> GetByTeacherIdAsync(Guid teacherId, CancellationToken ct = default);
        Task AddAsync(Assignment assignment, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

    }

   
}
