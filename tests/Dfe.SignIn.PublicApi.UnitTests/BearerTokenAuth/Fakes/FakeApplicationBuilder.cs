using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Dfe.SignIn.PublicApi.UnitTests.BearerTokenAuth.Fakes;

public class FakeApplicationBuilder : IApplicationBuilder
{
    // List of middleware actions that were added
    private readonly List<Func<RequestDelegate, RequestDelegate>> middlewares = [];

    public IServiceProvider ApplicationServices { get; set; } = new FakeServiceProvider();

    public IFeatureCollection Features { get; set; } = new FeatureCollection();

    public IFeatureCollection ServerFeatures => throw new NotImplementedException();

    public IDictionary<string, object?> Properties => throw new NotImplementedException();
    public IReadOnlyList<Func<RequestDelegate, RequestDelegate>> Middleware => this.middlewares.AsReadOnly();

    public FakeApplicationBuilder()
    {
        this.ApplicationServices = new FakeServiceProvider();
    }

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        this.middlewares.Add(middleware);
        return this;
    }

    public RequestDelegate Build()
    {
        RequestDelegate next = context => Task.CompletedTask;
        foreach (var middleware in this.middlewares.AsEnumerable().Reverse()) {
            next = middleware(next);
        }
        return next;
    }

    public IApplicationBuilder New()
    {
        throw new NotImplementedException();
    }
}

public class FakeServiceProvider : IServiceProvider
{
    public object GetService(Type serviceType) => null!;
}
