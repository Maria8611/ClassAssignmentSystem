using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Infrastructure.Persistence.Repositories
{
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

}
