namespace Dfe.SignIn.InternalApi.Endpoints;

/// <summary>
/// A compile-time safe registry for class-based Minimal API endpoints.
/// Handles both Dependency Injection registration and HTTP route mapping
/// using strongly-typed generics (no reflection).
/// </summary>
public sealed class EndpointRegistry
{
    private readonly List<(Type Type, ServiceLifetime Lifetime)> endpoints = [];
    private readonly List<Action<IEndpointRouteBuilder>> mappers = [];

    /// <summary>
    /// Registers an endpoint class for DI and captures its static Map delegate.
    /// </summary>
    /// <typeparam name="T">The endpoint class implementing <see cref="IEndpoint"/>.</typeparam>
    /// <param name="lifetime">The DI service lifetime. Defaults to Transient.</param>
    public EndpointRegistry Add<T>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, IEndpoint
    {
        this.endpoints.Add((typeof(T), lifetime));
        this.mappers.Add(T.Map);
        return this;
    }

    /// <summary>
    /// Registers all added endpoint classes with the DI container.
    /// </summary>
    public void RegisterServices(IServiceCollection services)
    {
        foreach (var (type, lifetime) in this.endpoints) {
            services.Add(new ServiceDescriptor(type, type, lifetime));
        }
    }

    /// <summary>
    /// Maps all added endpoint routes to the application.
    /// </summary>
    public void MapRoutes(IEndpointRouteBuilder app)
    {
        foreach (var map in this.mappers) {
            map(app);
        }
    }
}
