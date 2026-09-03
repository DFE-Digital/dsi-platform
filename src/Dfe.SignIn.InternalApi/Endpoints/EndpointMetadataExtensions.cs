namespace Dfe.SignIn.InternalApi.Endpoints;

/// <summary>
/// Provides extension methods for configuring standard responses and behaviors for API endpoints.
/// </summary>
public static class EndpointMetadataExtensions
{
    /// <summary>
    /// Configures standard responses and behaviors for an API endpoint, including common HTTP status codes, validation filters, and OpenAPI documentation.
    /// </summary>
    /// <typeparam name="TRequest"> The type of the request payload.</typeparam>
    /// <param name="builder"> The route handler builder.</param>
    /// <returns> The updated route handler builder.</returns>
    public static RouteHandlerBuilder WithStandardResponses<TRequest>(this RouteHandlerBuilder builder) where TRequest : notnull
    {
        return builder
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithValidationFilter<TRequest>()
            .WithOpenApi();
    }
}
