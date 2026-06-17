using FluentValidation;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Generic endpoint filter for FluentValidation.
/// </summary>
public class ValidationEndpointFilter<TRequest> : IEndpointFilter where TRequest : notnull
{
    /// <summary>
    /// Invokes the next endpoint filter in the pipeline after validating the request using a registered validator, if
    /// available.
    /// </summary>
    /// <remarks>If a validator for the request type is registered in the service provider, the request is
    /// validated before invoking the next filter. If validation fails, a BadRequest result containing the validation
    /// error messages is returned. Otherwise, the pipeline continues as normal.</remarks>
    /// <param name="context">The context for the current endpoint filter invocation, containing the HTTP context and request arguments.</param>
    /// <param name="next">The delegate representing the next filter or endpoint to invoke in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the response from the next filter or a
    /// BadRequest result if validation fails.</returns>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        if (validator is null) {
            return await next(context);
        }

        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null) {
            return await next(context);
        }

        var result = await validator.ValidateAsync(request);
        if (!result.IsValid) {
            // Join all error messages into a single string (plain BadRequest)
            var error = string.Join(" ", result.Errors.Select(e => e.ErrorMessage));
            return Results.BadRequest(error);
        }

        return await next(context);
    }
}

/// <summary>
/// Extension method to add the validation filter to the endpoint builder.
/// </summary>
public static class ValidationEndpointFilterExtensions
{
    /// <summary>
    /// Adds the validation filter to the endpoint builder for the specified request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to validate.</typeparam>
    /// <param name="builder">The endpoint builder to add the filter to.</param>
    /// <returns>The updated endpoint builder.</returns>
    public static RouteHandlerBuilder WithValidationFilter<TRequest>(this RouteHandlerBuilder builder) where TRequest : notnull
        => builder.AddEndpointFilter<ValidationEndpointFilter<TRequest>>();
}
