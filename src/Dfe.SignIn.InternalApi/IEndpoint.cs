namespace Dfe.SignIn.InternalApi;

/// <summary>
/// Defines a contract for an endpoint that can be mapped to an <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    static abstract void Map(IEndpointRouteBuilder app);
}
