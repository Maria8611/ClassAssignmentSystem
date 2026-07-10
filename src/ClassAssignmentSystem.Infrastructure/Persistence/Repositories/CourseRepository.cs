using ClassAssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Infrastructure.Persistence.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _db;
        public CourseRepository(AppDbContext db) => _db = db;
        public Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.Courses.Include(c => c.Teacher).FirstOrDefaultAsync(c => c.Id == id, ct);
        public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken ct = default)
            => await _db.Courses.Include(c => c.Teacher).OrderBy(c => c.Title).ToListAsync(ct);
        public async Task<IReadOnlyList<Course>> GetByTeacherIdAsync(Guid teacherId, CancellationToken ct = default)
            => await _db.Courses.Where(c => c.TeacherId == teacherId).Include(c => c.Teacher).ToListAsync(ct);
        public async Task AddAsync(Course course, CancellationToken ct = default) => await _db.Courses.AddAsync(course, ct);
        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
