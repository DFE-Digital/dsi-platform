namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Represents an error with a code, description, and optional target.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Description">The error description.</param>
/// <param name="Target">The target of the error, if applicable.</param>
public sealed record Error(string Code, string Description, string? Target = null)
{
    /// <summary>
    /// Represents a successful result with no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}

/// <summary>
/// Represents the result of an operation, indicating success or failure and containing an error if applicable.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with the specified success status and error.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error, if applicable.</param>
    /// <exception cref="ArgumentException">Thrown when the success status and error are inconsistent.</exception>
    protected Result(bool isSuccess, Error error)
    {
        if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None)) {
            throw new ArgumentException("Invalid error", nameof(error));
        }
        this.IsSuccess = isSuccess;
        this.Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !this.IsSuccess;

    /// <summary>
    /// Gets the error associated with the operation, if applicable.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result with no error.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error, if applicable.</param>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with the specified error and no value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="error">The error, if applicable.</param>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Defines an implicit conversion from an <see cref="Error"/> to a failed <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="Result"/> containing the error.</returns>
    public static implicit operator Result(Error error)
    {
        return Failure(error);
    }
}

/// <summary>
/// Represents the result of an operation that returns a value, indicating success or failure and containing an error if applicable.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class with the specified value, success status, and error.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error, if applicable.</param>
    protected internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        this.value = value;
    }

    /// <summary>
    /// Gets the value of the result if the operation was successful; otherwise, throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    public TValue Value => this.IsSuccess
        ? this.value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Defines an implicit conversion from a value of type <typeparamref name="TValue"/> to a successful <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing the value.</returns>
    public static implicit operator Result<TValue>(TValue value)
    {
        return Success(value);
    }

    /// <summary>
    /// Defines an implicit conversion from an <see cref="Error"/> to a failed <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> containing the error.</returns>
    public static implicit operator Result<TValue>(Error error)
    {
        return Failure<TValue>(error);
    }
}
