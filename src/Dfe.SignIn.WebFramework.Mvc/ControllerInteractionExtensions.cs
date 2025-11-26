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
    /// <param name="controller">The controller initiating the interaction.</param>
    /// <param name="viewModel">The view model containing input data.</param>
    /// <returns>
    ///   <para>A <see cref="MapInteractionRequestBuilder{TRequest}"/> that encapsulates the
    ///   controller, the populated request model, and the view model type for validation
    ///   mapping.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="controller"/> is null.</para>
    ///   <para>- or </para>
    ///   <para>If <paramref name="viewModel"/> is null.</para>
    /// </exception>
    public static MapInteractionRequestBuilder<TRequest> MapInteractionRequest<TRequest>(
        this Controller controller, object viewModel)
        where TRequest : class
    {
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
        return new(controller, request, viewModel.GetType());
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
                controller.ModelState.AddModelError(string.Empty, result.ErrorMessage!);
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
/// Provides a fluent interface for dispatching an interaction request built from a view
/// model, with support for request customization and validation error mapping.
/// </summary>
/// <typeparam name="TRequest">The type of request model.</typeparam>
/// <param name="controller">The controller initiating the interaction.</param>
/// <param name="request">The populated request model to dispatch.</param>
/// <param name="viewModelType">The type of the view model used for validation error mapping.</param>
public struct MapInteractionRequestBuilder<TRequest>(
    Controller controller, TRequest request, Type viewModelType)
    where TRequest : class
{
    /// <summary>
    /// Applies a transformation to the request model before dispatching.
    /// </summary>
    /// <param name="overrideDelegate">A delegate that returns a modified version of the request.</param>
    /// <returns>
    ///   <para>A new <see cref="MapInteractionRequestBuilder{TRequest}"/> instance with
    ///   the overridden request.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="overrideDelegate"/> is null.</para>
    /// </exception>
    public readonly MapInteractionRequestBuilder<TRequest> Use(Func<TRequest, TRequest> overrideDelegate)
    {
        ExceptionHelpers.ThrowIfArgumentNull(overrideDelegate, nameof(overrideDelegate));

        return new(controller, overrideDelegate(request), viewModelType);
    }

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
    /// <typeparam name="TResponse">The expected response type from the interaction.</typeparam>
    /// <param name="dispatch">The interaction dispatcher delegate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///   <para>The response from the interaction, or <c>null</c> if validation failed.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="dispatch"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidRequestException">
    ///   <para>Rethrown if validation fails but no specific validation results are available.</para>
    /// </exception>
    public readonly async Task<TResponse> InvokeAsync<TResponse>(
        InteractionDispatcher<TRequest> dispatch, CancellationToken cancellationToken = default)
        where TResponse : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(dispatch, nameof(dispatch));

        try {
            return await dispatch(request, cancellationToken).To<TResponse>();
        }
        catch (InvalidRequestException ex) {
            if (!ex.ValidationResults.Any()) {
                throw;
            }
            controller.MapRequestValidationResults<TRequest>(viewModelType, ex.ValidationResults);
            return null!;
        }
    }

    /// <summary>
    /// Dispatches the interaction request asynchronously and maps any validation errors
    /// back to the controller's <see cref="ControllerBase.ModelState"/>.
    /// </summary>
    /// <param name="dispatch">The interaction dispatcher delegate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="dispatch"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidRequestException">
    ///   <para>Rethrown if validation fails but no specific validation results are available.</para>
    /// </exception>
    public readonly Task InvokeAsync(InteractionDispatcher<TRequest> dispatch, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<object>(dispatch, cancellationToken);
    }
}

/// <summary>
/// Represents a method that dispatches an interaction using a request model.
/// </summary>
/// <typeparam name="TRequest">The type of the request model.</typeparam>
/// <param name="request">The request model.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>
///   <para>A <see cref="InteractionTask"/> representing the asynchronous interaction.</para>
/// </returns>
/// <exception cref="OperationCanceledException" />
public delegate InteractionTask InteractionDispatcher<in TRequest>(TRequest request, CancellationToken cancellationToken)
    where TRequest : class;
