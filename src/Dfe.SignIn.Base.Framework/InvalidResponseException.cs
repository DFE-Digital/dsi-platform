using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// The exception that occurs when one or more validation errors are encountered whilst
/// validating the response of an interaction (eg. use case, api request, etc).
/// </summary>
public class InvalidResponseException : InteractionException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidResponseException"/> class.
    /// </summary>
    public InvalidResponseException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidResponseException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public InvalidResponseException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidResponseException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public InvalidResponseException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidResponseException"/> class.
    /// </summary>
    /// <param name="invocationId">The unique ID of the invocation.</param>
    /// <param name="validationResults">The validation results.</param>
    public InvalidResponseException(Guid invocationId, IEnumerable<ValidationResult> validationResults)
    {
        this.InvocationId = invocationId;
        this.ValidationResults = validationResults;
    }

    /// <summary>
    /// Gets the unique ID of the invocation.
    /// </summary>
    [Persist]
    public Guid InvocationId { get; init; }

    /// <summary>
    /// Gets the collection of validation results.
    /// </summary>
    [Persist]
    public IEnumerable<ValidationResult> ValidationResults { get; init; } = [];
}
