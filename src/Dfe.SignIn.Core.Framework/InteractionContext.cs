using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Provides context for an interaction.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <param name="request">The interaction request model.</param>
public class InteractionContext<TRequest>(TRequest request)
    where TRequest : class
{
    /// <summary>
    /// Gets the unique ID of the interactor invocation.
    /// </summary>
    public Guid InvocationId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the request model of the interaction.
    /// </summary>
    public TRequest Request { get; } = request;

#pragma warning disable IDE0028 // A mutable collection type is required here.
    /// <summary>
    /// Gets the list of any validation results that have accumulated during the invocation
    /// of the interaction.
    /// </summary>
    public ICollection<ValidationResult> ValidationResults { get; } = new List<ValidationResult>();
#pragma warning restore IDE0028

    /// <summary>
    /// Adds a validation error to the interaction invocation context.
    /// </summary>
    /// <param name="errorMessage">An user-friendly error message.</param>
    /// <param name="propertyName">Name of the model property.</param>
    public void AddValidationError(string? errorMessage, string? propertyName = null)
    {
        this.ValidationResults.Add(
            !string.IsNullOrWhiteSpace(propertyName)
                ? new(errorMessage, [propertyName])
                : new(errorMessage)
        );
    }

    /// <summary>
    /// Throws an <see cref="InvalidRequestException"/> exception if one or more validation
    /// errors have been recorded in the <see cref="ValidationResults"/> property.
    /// </summary>
    /// <exception cref="InvalidRequestException">
    ///   <para>If one or more validation errors have been recorded.</para>
    /// </exception>
    public void ThrowIfHasValidationErrors()
    {
        if (this.ValidationResults.Count != 0) {
            throw new InvalidRequestException(this.InvocationId, this.ValidationResults);
        }
    }

    /// <summary>
    /// An implicit operator to wrap a request model within an interaction context.
    /// </summary>
    /// <remarks>
    ///   <para>This is a developer convenience to make it easier (less verbose) to write
    ///   unit tests for interactors.</para>
    /// </remarks>
    /// <param name="request">The request model.</param>
    public static implicit operator InteractionContext<TRequest>(TRequest request)
    {
        return new InteractionContext<TRequest>(request);
    }
}
