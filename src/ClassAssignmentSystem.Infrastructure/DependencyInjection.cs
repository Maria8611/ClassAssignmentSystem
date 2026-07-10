using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ClassAssignmentSystem.Application.Common.Interfaces;

namespace ClassAssignmentSystem.Infrastructure;

/// <summary>
/// Infrastructure layer service registration.
///
/// Called once from Program.cs:
///   builder.Services.AddInfrastructure(builder.Configuration);
///
/// What gets registered here:
///   - AppDbContext (Scoped — one instance per HTTP request)
///   - UnitOfWork   (Scoped — must share lifetime with AppDbContext)
///   - Repositories (Scoped — registered as each feature is added)
///
/// Connection string is read from appsettings.json under "ConnectionStrings:DefaultConnection".
/// Never hardcode connection strings — they should come from configuration
/// or environment variables in production.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core + SQL Server ──────────────────────────────────────────
        services.AddDbContext<Persistence.AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    // Retry on transient failures (network blips, SQL Server restarts).
                    // 3 retries with exponential back-off — suitable for most scenarios.
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

        // ── Unit of Work ──────────────────────────────────────────────────
        // Scoped lifetime matches AppDbContext — they share the same instance per request.
        services.AddScoped<IUnitOfWork, Persistence.UnitOfWork>();

        // ── Repositories ──────────────────────────────────────────────────
        // Registered as each aggregate's repository is implemented, e.g.:
        // services.AddScoped<ICourseRepository, CourseRepository>();
        // services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        // services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        return services;
    }
}