namespace ClassAssignmentSystem.Application.Common.Results;

/// <summary>
/// Represents a structured error — a typed failure with a machine-readable code
/// and a human-readable description.
///
/// Why not just throw exceptions?
///   Exceptions are for UNEXPECTED failures (null ref, DB timeout).
///   Business rule violations — "student already submitted", "deadline has passed" —
///   are EXPECTED outcomes. Modeling them as errors keeps the happy path clean,
///   forces callers to handle failures explicitly, and makes handlers testable
///   without try/catch in tests.
///
/// ErrorType drives which HTTP status code the API layer returns:
///   Validation   → 400 Bad Request
///   NotFound     → 404 Not Found
///   Conflict     → 409 Conflict
///   Forbidden    → 403 Forbidden
///   Failure      → 500 Internal Server Error (unexpected domain failure)
/// </summary>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    /// <summary>
    /// Represents the absence of an error. Used as the default on a successful Result.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    // ── Convenience factories ─────────────────────────────────────────────
    // These keep error construction consistent and readable at the call site:
    //   Error.NotFound("Course.NotFound", $"Course {id} was not found")

    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation);

    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);

    public static Error Forbidden(string code, string description)
        => new(code, description, ErrorType.Forbidden);

    public static Error Failure(string code, string description)
        => new(code, description, ErrorType.Failure);
}

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Forbidden
}