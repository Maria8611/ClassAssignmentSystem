using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClassAssignmentSystem.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext db, IPasswordHasher hasher, ILogger<DatabaseSeeder> logger)
    { _db = db; _hasher = hasher; _logger = logger; }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await _db.Database.MigrateAsync(ct);
        if (await _db.Users.AnyAsync(u => u.Role == UserRole.Admin, ct))
        {
            _logger.LogInformation("Admin already exists. Skipping seed.");
            return;
        }
        var admin = User.CreateAdmin("System Administrator", "admin@classassign.local", _hasher.Hash("Admin@123456"));
        await _db.Users.AddAsync(admin, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded default admin: admin@classassign.local / Admin@123456");
    }
}
