using FluentValidation;
using MediatR;
using ClassAssignmentSystem.Application.Common.Results;

namespace ClassAssignmentSystem.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically runs FluentValidation
/// for every Command or Query BEFORE the handler executes.
///
/// How the pipeline works:
///   Controller → MediatR.Send(command)
///     → [ValidationBehavior]  ← runs here first
///     → [LoggingBehavior]     ← optional, runs next
///     → CommandHandler        ← only reached if validation passes
///
/// Why here instead of in the controller?
///   - Validation is application concern, not HTTP concern
///   - Handlers are tested in isolation — the behavior runs automatically in tests too
///   - No duplication: one validator per command, always runs, no forget risk
///   - Controllers stay thin — they never call validator.Validate() manually
///
/// What happens on failure?
///   Returns a Result.Failure with ErrorType.Validation — the API layer maps
///   this to HTTP 400 with structured error details. No exception thrown.
///
/// What if no validator is registered for a command?
///   IEnumerable&lt;IValidator&lt;TRequest&gt;&gt; will be empty — the behavior passes through
///   without running any validation. Commands without validators are valid by default.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // No validators registered for this request type → pass through
        if (!_validators.Any())
            return await next();

        // Run all validators concurrently and collect results
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(request, cancellationToken)));

        // Flatten all failures across all validators into a single list
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // Build a structured validation error from the first failure.
        // The PropertyName becomes the code (e.g. "Title"), the ErrorMessage the description.
        // For multiple failures, consider building a composite error — but keep it simple for now.
        var firstFailure = failures[0];

        var error = Error.Validation(
            code: firstFailure.PropertyName,
            description: firstFailure.ErrorMessage);

        // We need to return TResponse (which is a Result subtype) carrying the error.
        // TResponse is constrained to Result, so we create it via reflection.
        // This is the standard approach for generic Result + MediatR pipeline behaviors.
        return CreateValidationResult<TResponse>(error);
    }

    private static TResponse CreateValidationResult<TResult>(Error error)
        where TResult : Result<TResult>
    {
        // If TResult is non-generic Result, call Result.Failure(error)
        if (typeof(TResult) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        // If TResult is Result<T>, call Result<T>.Failure(error) via reflection
        // This is necessary because we don't know T at compile time in the behavior.
        var genericType = typeof(TResult).GetGenericArguments()[0];
        var resultType = typeof(Result<>).MakeGenericType(genericType);
        var failureMethod = resultType.GetMethod(
            nameof(Result<object>.Failure),
            [typeof(Error)])!;

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}