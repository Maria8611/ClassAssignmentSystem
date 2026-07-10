using MediatR;
using Microsoft.EntityFrameworkCore;
using ClassAssignmentSystem.Domain.Common;
using ClassAssignmentSystem.Domain.Entities;

namespace ClassAssignmentSystem.Infrastructure.Persistence;

/// <summary>
/// The single EF Core DbContext for the application.
///
/// Responsibilities:
///   1. Exposes DbSet&lt;T&gt; for each aggregate root (added as features are built)
///   2. Applies entity configurations from separate IEntityTypeConfiguration&lt;T&gt; classes
///   3. Automatically stamps CreatedAt / UpdatedAt on every save
///   4. Dispatches domain events AFTER SaveChanges succeeds
///
/// What this class does NOT do:
///   - No business logic — that lives in the domain
///   - No manual column mapping — all mapping is in Configurations/
///   - Not injected into Application layer — Application uses IUnitOfWork + IRepository
///
/// Domain event dispatch order:
///   1. Collect all domain events from tracked aggregate roots
///   2. Call base.SaveChangesAsync() → persists to DB
///   3. Dispatch each event via IPublisher (MediatR)
///   4. Clear events from entities so they are not re-dispatched on next save
///
///   Events are dispatched AFTER commit — this means handlers see a consistent DB state.
///   The trade-off: if dispatch throws, the DB is already committed. For this portfolio
///   project that is acceptable. Production systems often use an Outbox pattern instead.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IPublisher _publisher;

    // DbSets will be added here as aggregate roots are implemented, e.g.:
    public DbSet<Course> Courses { get; set; } = null!;
    //public DbSet<Assignment> Assignments { get; set; } = null!;
    //public DbSet<Submission> Submissions { get; set; } = null!;
    //public DbSet<Student> Students { get; set; } = null!;
    //public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<User> Users => Set<User>();

    public DbSet<EnrollmentRequest> EnrollmentRequests => Set<EnrollmentRequest>();

    public AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scans the Infrastructure assembly for all IEntityTypeConfiguration<T>
        // implementations and applies them. This keeps OnModelCreating clean —
        // never add column mappings directly in this method.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(255);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).IsRequired();
            e.Property(u => u.IsActive).IsRequired();
        });

        modelBuilder.Entity<Course>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Title).IsRequired().HasMaxLength(200);
            e.Property(c => c.Description).IsRequired().HasMaxLength(1000);
            e.Property(c => c.TotalSeats).IsRequired();
            e.Property(c => c.Status).IsRequired();
            e.HasOne(c => c.Teacher)
             .WithMany(u => u.TaughtCourses)
             .HasForeignKey(c => c.TeacherId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EnrollmentRequest>(e =>
        {
            e.HasKey(er => er.Id);
            e.Property(er => er.Status).IsRequired();
            e.HasOne(er => er.Course)
             .WithMany(c => c.EnrollmentRequests)
             .HasForeignKey(er => er.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(er => er.Student)
             .WithMany(u => u.EnrollmentRequests)
             .HasForeignKey(er => er.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(er => new { er.StudentId, er.CourseId, er.Status });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ── 1. Stamp audit fields ─────────────────────────────────────────
        // EF change tracker knows which entities are Added or Modified.
        // We intercept before the SQL is generated and set the timestamps.

        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Use EF's SetProperty so shadow properties also work
                    entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = now;
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = now;
                    break;

                case EntityState.Modified:
                    // Never allow CreatedAt to be changed after creation
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = now;
                    break;
            }
        }

        // ── 2. Collect domain events before saving ────────────────────────
        // Collect now — after SaveChanges the ChangeTracker may be cleared.
        var domainEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .SelectMany(e =>
            {
                var events = e.DomainEvents.ToList();
                e.ClearDomainEvents(); // Clear before save so they are not re-dispatched
                return events;
            })
            .ToList();

        // ── 3. Persist to database ────────────────────────────────────────
        var result = await base.SaveChangesAsync(cancellationToken);

        // ── 4. Dispatch domain events post-commit ─────────────────────────
        // IPublisher dispatches to all INotificationHandler<TEvent> registered with MediatR.
        // These run in-process. For out-of-process messaging, push to a queue here instead.
        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);

        return result;
    }
}