using ClassAssignmentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Common.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Course>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task AddAsync(Assignment assignment, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
