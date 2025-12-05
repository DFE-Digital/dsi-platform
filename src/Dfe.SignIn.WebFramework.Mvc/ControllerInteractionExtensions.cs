using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.SignIn.WebFramework.Mvc;

/// <summary>
/// Provides extension methods for mapping view model data to interaction request models
/// and dispatching them through a controller.
/// </summary>
public static class ControllerInteractionExtensions
{
    /// <summary>
    /// Creates and populates an interaction request model from the specified view model,
    /// using <see cref="MapToAttribute{TRequest}"/> annotations to map properties.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request model to map to.</typeparam>
    /// <param name="interaction">The service for dispatching interaction requests.</param>
    /// <param name="controller">The controller initiating the interaction.</param>
    /// <param name="viewModel">The view model containing input data.</param>
    /// <returns>
    ///   <para>A <see cref="IMappedInteractionContextBuilder{TRequest}"/> for building
    ///   the mapped interaction request.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="interaction"/> is null.</para>
    ///   <para>- or </para>
    ///   <para>If <paramref name="controller"/> is null.</para>
    ///   <para>- or </para>
    ///   <para>If <paramref name="viewModel"/> is null.</para>
    /// </exception>
    public static IMappedInteractionContextBuilder<TRequest> MapRequestFromViewModel<TRequest>(
        this IInteractionDispatcher interaction, Controller controller, object viewModel)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(interaction, nameof(interaction));
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));
        ExceptionHelpers.ThrowIfArgumentNull(viewModel, nameof(viewModel));

        var request = Activator.CreateInstance<TRequest>();
        var mappings = RequestMappingHelpers.GetMappings<TRequest>(viewModel.GetType()).Values;
        foreach (var mapping in mappings) {
            if (mapping.Flags.HasFlag(RequestMappingOptions.Value)) {
                var sourceValue = mapping.ViewModelProperty.GetValue(viewModel);
                var targetValue = Convert.ChangeType(sourceValue, mapping.RequestProperty.PropertyType);
                mapping.RequestProperty.SetValue(request, targetValue);
            }
        }
        return new MappedInteractionContextBuilder<TRequest>(
            interaction, controller, viewModel.GetType(), request);
    }

    /// <summary>
    /// Maps any validation errors back to the controller's <see cref="ControllerBase.ModelState"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request model to map to.</typeparam>
    /// <param name="controller">The controller initiating the interaction.</param>
    /// <param name="viewModelType">The type of view model containing input data.</param>
    /// <param name="validationResults">The enumerable collection of validation results.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="controller"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="viewModelType"/> is null.</para>
    /// </exception>
    public static void MapRequestValidationResults<TRequest>(
        this Controller controller, Type viewModelType, IEnumerable<ValidationResult> validationResults)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));
        ExceptionHelpers.ThrowIfArgumentNull(viewModelType, nameof(viewModelType));

        var mappings = RequestMappingHelpers.GetMappings<TRequest>(viewModelType);
        foreach (var result in validationResults ?? []) {
            string? memberName = result.MemberNames.FirstOrDefault();
            if (!string.IsNullOrEmpty(memberName) && mappings.TryGetValue(memberName, out var mapping)) {
                controller.ModelState.AddModelError(mapping.ViewModelProperty.Name, result.ErrorMessage!);
            }
            else {
                throw new InvalidOperationException(
                    $"Unable to map validation result '{result.MemberNames.FirstOrDefault()}' with message '{result.ErrorMessage}'.");
            }
        }
    }

    /// <summary>
    /// Maps any validation errors back to the controller's <see cref="ControllerBase.ModelState"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request model to map to.</typeparam>
    /// <param name="controller">The controller initiating the interaction.</param>
    /// <param name="viewModel">The view model containing input data.</param>
    /// <param name="validationResults">The enumerable collection of validation results.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="controller"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="viewModel"/> is null.</para>
    /// </exception>
    public static void MapRequestValidationResults<TRequest>(
        this Controller controller, object viewModel, IEnumerable<ValidationResult> validationResults)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(viewModel, nameof(viewModel));

        MapRequestValidationResults<TRequest>(controller, viewModel.GetType(), validationResults);
    }
}

/// <summary>
/// Defines a fluent builder for configuring and dispatching interactions where
/// the request is mapped from properties of a view model.
/// </summary>
public interface IMappedInteractionContextBuilder<TRequest>
    where TRequest : class
{
    /// <inheritdoc cref="IInteractionContextBuilder.WithCancellation(CancellationToken)"/>
    /// <returns>
    ///   <para>The <see cref="IMappedInteractionContextBuilder{TRequest}"/> instance for chained calls.</para>
    /// </returns>
    IMappedInteractionContextBuilder<TRequest> WithCancellation(CancellationToken cancellationToken);

    /// <inheritdoc cref="IInteractionContextBuilder.IgnoreCache(bool)"/>
    /// <returns>
    ///   <para>The <see cref="IMappedInteractionContextBuilder{TRequest}"/> instance for chained calls.</para>
    /// </returns>
    IMappedInteractionContextBuilder<TRequest> IgnoreCache(bool ignoreHint = true);

    /// <summary>
    /// Applies a transformation to the request model before dispatching.
    /// </summary>
    /// <param name="overrideDelegate">A delegate that returns a modified version of the request.</param>
    /// <returns>
    ///   <para>The <see cref="IMappedInteractionContextBuilder{TRequest}"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="overrideDelegate"/> is null.</para>
    /// </exception>
    IMappedInteractionContextBuilder<TRequest> Use(Func<TRequest, TRequest> overrideDelegate);

    /// <summary>
    /// Dispatches the interaction request asynchronously and maps any validation errors
    /// back to the controller's <see cref="ControllerBase.ModelState"/>.
    /// </summary>
    /// <remarks>
    ///   <para>This method returns a non-nullable value for convenience, even though the
    ///   result may be <c>null</c> if validation fails. When <see cref="ModelStateDictionary.IsValid"/>
    ///   is <c>true</c>, the returned value is guaranteed to be non-null. Consumers should
    ///   always check <see cref="ModelStateDictionary.IsValid"/> before accessing the result to ensure
    ///   it is safe to use.</para>
    /// </remarks>
    /// <returns>
    ///   <para>The response from the interaction, or <c>null</c> if validation failed.</para>
    /// </returns>
    /// <exception cref="InvalidRequestException">
    ///   <para>Rethrown if validation fails but no specific validation results are available.</para>
    /// </exception>
    InteractionTask DispatchAsync();
}

/// <summary>
/// Provides a fluent interface for dispatching an interaction request built from a view
/// model, with support for request customization and validation error mapping.
/// </summary>
/// <typeparam name="TRequest">The type of request model.</typeparam>
/// <param name="interaction">The service to dispatch interaction requests.</param>
/// <param name="controller">The controller initiating the interaction.</param>
/// <param name="viewModelType">The type of the view model used for validation error mapping.</param>
/// <param name="request">The populated request model to dispatch.</param>
public sealed class MappedInteractionContextBuilder<TRequest>(
    IInteractionDispatcher interaction, Controller controller, Type viewModelType, TRequest request
) : IMappedInteractionContextBuilder<TRequest> where TRequest : class
{
    private IInteractionContextBuilder builder = interaction.Build();

    /// <inheritdoc/>
    public IMappedInteractionContextBuilder<TRequest> WithCancellation(CancellationToken cancellationToken)
    {
        this.builder = this.builder.WithCancellation(cancellationToken);
        return this;
    }

    /// <inheritdoc/>
    public IMappedInteractionContextBuilder<TRequest> IgnoreCache(bool ignoreHint = true)
    {
        this.builder = this.builder.IgnoreCache(ignoreHint);
        return this;
    }

    /// <inheritdoc/>
    public IMappedInteractionContextBuilder<TRequest> Use(Func<TRequest, TRequest> overrideDelegate)
    {
        ExceptionHelpers.ThrowIfArgumentNull(overrideDelegate, nameof(overrideDelegate));
        request = overrideDelegate(request);
        return this;
    }

    /// <inheritdoc/>
    public InteractionTask DispatchAsync()
    {
        async Task<object> InvokeAsync()
        {
            try {
                return await this.builder.DispatchAsync(request).To<object>();
            }
            catch (InvalidRequestException ex) {
                if (!ex.ValidationResults.Any()) {
                    throw;
                }
                controller.MapRequestValidationResults<TRequest>(viewModelType, ex.ValidationResults);
                return null!;
            }
        }
        return new InteractionTask(InvokeAsync());
    }
}
