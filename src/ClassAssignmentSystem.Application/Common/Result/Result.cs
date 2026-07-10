namespace ClassAssignmentSystem.Application.Common.Results;

/// <summary>
/// Non-generic Result — for operations that succeed or fail with no return value.
/// Example: PublishAssignmentCommand returns Result, not Result&lt;Guid&gt;.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        // Invariant: a success result must carry Error.None,
        // a failure result must carry a real error.
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot contain an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must contain an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    // ── Factory methods ───────────────────────────────────────────────────
    // Always use these — never call the constructor directly.

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Implicit conversion from Error → Result so handlers can write:
    ///   return DomainErrors.Course.NotFound;
    /// instead of:
    ///   return Result.Failure(DomainErrors.Course.NotFound);
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Generic Result — for operations that return a value on success.
/// Example: CreateCourseCommandHandler returns Result&lt;Guid&gt; (the new course Id).
///
/// Callers check IsSuccess before accessing Value:
///   var result = await handler.Handle(command);
///   if (result.IsFailure) return result.Error;
///   var courseId = result.Value;
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// The success value. Throws if accessed on a failed result.
    /// Always check IsSuccess before accessing.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    // ── Factory methods ───────────────────────────────────────────────────

    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);

    public new static Result<TValue> Failure(Error error) => new(default, false, error);

    /// <summary>
    /// Implicit conversion from TValue → Result&lt;TValue&gt; so handlers can write:
    ///   return courseId;
    /// instead of:
    ///   return Result&lt;Guid&gt;.Success(courseId);
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Implicit conversion from Error → Result&lt;TValue&gt; so handlers can write:
    ///   return DomainErrors.Course.NotFound;
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}