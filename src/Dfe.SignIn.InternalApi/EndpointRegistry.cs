using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi;

/// <summary>
/// A compile-time safe registry for class-based Minimal API endpoints.
/// Handles both Dependency Injection registration and HTTP route mapping
/// using strongly-typed generics (no reflection).
/// </summary>
public sealed class EndpointRegistry
{
    private readonly List<(Type Type, ServiceLifetime Lifetime)> _endpoints = [];
    private readonly List<Action<IEndpointRouteBuilder>> _mappers = [];

    /// <summary>
    /// Registers an endpoint class for DI and captures its static Map delegate.
    /// </summary>
    /// <typeparam name="T">The endpoint class implementing <see cref="IEndpoint"/>.</typeparam>
    /// <param name="lifetime">The DI service lifetime. Defaults to Transient.</param>
    public EndpointRegistry Add<T>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, IEndpoint
    {
        _endpoints.Add((typeof(T), lifetime));
        _mappers.Add(T.Map);
        return this;
    }

    /// <summary>
    /// Registers all added endpoint classes with the DI container.
    /// </summary>
    public void RegisterServices(IServiceCollection services)
    {
        foreach (var (type, lifetime) in _endpoints)
        {
            services.Add(new ServiceDescriptor(type, type, lifetime));
        }
    }

    /// <summary>
    /// Maps all added endpoint routes to the application.
    /// </summary>
    public void MapRoutes(IEndpointRouteBuilder app)
    {
        foreach (var map in _mappers)
        {
            map(app);
        }
    }
}
