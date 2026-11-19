using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Dfe.SignIn.AppHost.UnitTests;

[TestClass]
public sealed class RedisResourceBuilderExtensionsTests
{
    [TestMethod]
    [DataRow("localhost", 16379)]
    [DataRow("redis.local", 26379)]
    public async Task ConnectionStrings_ResolveAllocatedEndpointInEachClientFormat(string host, int port)
    {
        var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions {
            Args = [],
            DisableDashboard = true
        });
        var redis = builder.AddPlatformRedis("redis");
        var dotnetConnectionString = redis.GetDotnetConnectionString();
        var nodeConnectionString = redis.GetNodeConnectionString();
        var endpoint = redis.Resource.Annotations.OfType<EndpointAnnotation>().Single(annotation => annotation.Name == "tcp");
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, host, port);

        var dotnetValue = await ((IValueProvider)dotnetConnectionString).GetValueAsync(default);
        var nodeValue = await ((IValueProvider)nodeConnectionString).GetValueAsync(default);

        Assert.AreEqual($"{host}:{port}", dotnetValue);
        Assert.AreEqual($"redis://{host}:{port}", nodeValue);
    }
}
