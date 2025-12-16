using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Represents a service that validates the request and response models of an interaction.
/// </summary>
public interface IInteractionValidator
{
    /// <summary>
    /// Attempts to validate a request model.
    /// </summary>
    /// <param name="requestModel">The request model instance.</param>
    /// <param name="validationResults">A mutable collection that will be updated with results.</param>
    /// <returns>
    ///   <para>A value of <c>true</c> if the model is valid; otherwise, a value of <c>false</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="requestModel"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="validationResults"/> is null.</para>
    /// </exception>
    bool TryValidateRequest(object requestModel, ICollection<ValidationResult> validationResults);

    /// <summary>
    /// Attempts to validate a response model.
    /// </summary>
    /// <param name="responseModel">The response model instance.</param>
    /// <param name="validationResults">A mutable collection that will be updated with results.</param>
    /// <returns>
    ///   <para>A value of <c>true</c> if the model is valid; otherwise, a value of <c>false</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="responseModel"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="validationResults"/> is null.</para>
    /// </exception>
    bool TryValidateResponse(object responseModel, ICollection<ValidationResult> validationResults);
}

/// <summary>
/// A concrete implementation of <see cref="IInteractionValidator"/> which uses the data
/// annotations feature of .NET Core to validate the properties of request and response
/// models.
/// </summary>
/// <param name="services">The service provider.</param>
public sealed class InteractionValidator(IServiceProvider services) : IInteractionValidator
{
    /// <inheritdoc/>
    public bool TryValidateRequest(object requestModel, ICollection<ValidationResult> validationResults)
    {
        ExceptionHelpers.ThrowIfArgumentNull(requestModel, nameof(requestModel));
        ExceptionHelpers.ThrowIfArgumentNull(validationResults, nameof(validationResults));

        return this.TryValidate(requestModel, validationResults);
    }

    /// <inheritdoc/>
    public bool TryValidateResponse(object responseModel, ICollection<ValidationResult> validationResults)
    {
        ExceptionHelpers.ThrowIfArgumentNull(responseModel, nameof(responseModel));
        ExceptionHelpers.ThrowIfArgumentNull(validationResults, nameof(validationResults));

        return this.TryValidate(responseModel, validationResults);
    }

    private bool TryValidate(object model, ICollection<ValidationResult> validationResults)
    {
        var context = new ValidationContext(model);
        context.InitializeServiceProvider(services.GetService);

        return Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
    }
}
