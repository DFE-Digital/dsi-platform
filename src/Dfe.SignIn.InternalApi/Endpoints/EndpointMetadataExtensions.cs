namespace Dfe.SignIn.InternalApi.Endpoints;

/// <summary>
/// Provides extension methods for configuring standard responses and behaviors for API endpoints.
/// </summary>
public static class EndpointMetadataExtensions
{
    /// <summary>
    /// Configures standard responses and behaviors for an API endpoint, including common HTTP status codes, validation filters, and OpenAPI documentation.
    /// </summary>
    /// <param name="builder"> The route handler builder.</param>
    /// <returns> The updated route handler builder.</returns>
    public static RouteHandlerBuilder WithStandardResponses(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces(StatusCodes.Status200OK)
            .WithStandardProblems()
            .WithOpenApi();
    }

    /// <summary>
    /// Configures standard responses and behaviors for an API endpoint that returns a specific response type, including common HTTP status codes, validation filters, and OpenAPI documentation.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The updated route handler builder.</returns>
    public static RouteHandlerBuilder WithStandardResponses<TResponse>(this RouteHandlerBuilder builder)
        where TResponse : class
    {
        return builder
            .Produces<TResponse>(StatusCodes.Status200OK)
            .WithStandardProblems()
            .WithOpenApi();
    }

    private static RouteHandlerBuilder WithStandardProblems(this RouteHandlerBuilder builder)
    {
        return builder
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi();
    }
}
