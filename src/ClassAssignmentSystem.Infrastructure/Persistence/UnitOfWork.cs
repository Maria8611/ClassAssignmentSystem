using ClassAssignmentSystem.Application.Common.Interfaces;

namespace ClassAssignmentSystem.Infrastructure.Persistence;

/// <summary>
/// Concrete implementation of IUnitOfWork backed by AppDbContext.
///
/// This is intentionally thin — it delegates directly to AppDbContext.SaveChangesAsync(),
/// which handles audit stamping and domain event dispatch internally.
///
/// Why have this class at all if it's just a wrapper?
///   The Application layer depends on IUnitOfWork — an interface it defines.
///   This keeps Application decoupled from EF Core entirely.
///   Command handlers call IUnitOfWork.SaveChangesAsync() without knowing
///   whether the underlying store is SQL Server, Postgres, or an in-memory fake.
///
/// Lifetime: Scoped — same lifetime as AppDbContext, so they share the same
/// DbContext instance within a single HTTP request. This is critical: if
/// UnitOfWork had a different DbContext instance than the repository,
/// the repository's staged changes would not be visible to SaveChanges.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}