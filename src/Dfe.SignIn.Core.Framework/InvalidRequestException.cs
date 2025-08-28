using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// The exception that occurs when one or more validation errors are encountered whilst
/// processing an interaction (eg. use case, api request, etc).
/// </summary>
public class InvalidRequestException : InteractionException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestException"/> class.
    /// </summary>
    public InvalidRequestException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public InvalidRequestException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public InvalidRequestException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestException"/> class.
    /// </summary>
    /// <param name="invocationId">The unique ID of the invocation.</param>
    /// <param name="validationResults">The validation results.</param>
    public InvalidRequestException(Guid invocationId, IEnumerable<ValidationResult> validationResults)
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
