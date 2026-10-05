namespace Dfe.SignIn.Base.Framework.OperationResults;

/// <summary>
/// Represents the result of an operation, indicating success or failure and containing an error if applicable.
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult"/> class with the specified success status and error.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error, if applicable.</param>
    /// <exception cref="ArgumentException">Thrown when the success status and error are inconsistent.</exception>
    protected OperationResult(bool isSuccess, OperationError error)
    {
        if ((isSuccess && error != OperationError.None) || (!isSuccess && error == OperationError.None)) {
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
    public OperationError Error { get; }

    /// <summary>
    /// Creates a successful result with no error.
    /// </summary>
    public static OperationResult Success() => new(true, OperationError.None);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    public static OperationResult<TValue> Success<TValue>(TValue value) => new(value, true, OperationError.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error, if applicable.</param>
    public static OperationResult Failure(OperationError error) => new(false, error);

    /// <summary>
    /// Creates a failed result with the specified error and no value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="error">The error, if applicable.</param>
    public static OperationResult<TValue> Failure<TValue>(OperationError error) => new(default, false, error);

    /// <summary>
    /// Defines an implicit conversion from an <see cref="Error"/> to a failed <see cref="OperationResult"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="OperationResult"/> containing the error.</returns>
    public static implicit operator OperationResult(OperationError error)
    {
        return Failure(error);
    }
}

/// <summary>
/// Represents the result of an operation that returns a value, indicating success or failure and containing an error if applicable.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class OperationResult<TValue> : OperationResult
{
    private readonly TValue? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult{TValue}"/> class with the specified value, success status, and error.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error, if applicable.</param>
    protected internal OperationResult(TValue? value, bool isSuccess, OperationError error) : base(isSuccess, error)
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
    /// Defines an implicit conversion from a value of type <typeparamref name="TValue"/> to a successful <see cref="OperationResult{TValue}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful <see cref="OperationResult{TValue}"/> containing the value.</returns>
    public static implicit operator OperationResult<TValue>(TValue value)
    {
        return Success(value);
    }

    /// <summary>
    /// Defines an implicit conversion from an <see cref="OperationError"/> to a failed <see cref="OperationResult{TValue}"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="OperationResult{TValue}"/> containing the error.</returns>
    public static implicit operator OperationResult<TValue>(OperationError error)
    {
        return Failure<TValue>(error);
    }
}
