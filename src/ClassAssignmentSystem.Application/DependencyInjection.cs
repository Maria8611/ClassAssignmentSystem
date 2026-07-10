using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ClassAssignmentSystem.Application.Common.Behaviors;

namespace ClassAssignmentSystem.Application;

/// <summary>
/// Application layer service registration.
///
/// Called once from Program.cs:
///   builder.Services.AddApplication();
///
/// This keeps Program.cs clean and makes the Application layer self-describing —
/// it declares its own dependencies rather than relying on the host to know them.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR: scans this assembly for all IRequestHandler<,> implementations.
        // Commands and Queries don't need manual registration — MediatR finds them.
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            // Register the ValidationBehavior as an open-generic pipeline behavior.
            // MediatR will wrap EVERY handler with this behavior automatically.
            // Order matters: validation runs before any other behavior.
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation: scans this assembly for all AbstractValidator<T> implementations.
        // Validators don't need manual registration either.
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}