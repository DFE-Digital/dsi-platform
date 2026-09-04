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
    public static RouteHandlerBuilder WithStandardResponses(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();
    }
}
