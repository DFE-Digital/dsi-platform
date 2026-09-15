namespace Dfe.SignIn.AppHost.Resources;

/// <summary>
/// Shared infrastructure resources and derived connection strings used by apps.
/// </summary>
public sealed record PlatformInfrastructure(
    IResourceBuilder<RedisResource> Redis,
    ReferenceExpression DotNetRedisConnectionString,
    ReferenceExpression NodeRedisConnectionString,
    IResourceBuilder<ContainerResource> Frontend,
    EndpointReference FrontendEndpoint);

/// <summary>
/// Registers Redis and frontend container resources for the local platform.
/// </summary>
public static class PlatformInfrastructureExtensions
{
    /// <summary>
    /// Adds Redis and the frontend asset container, returning connection helpers.
    /// </summary>
    public static PlatformInfrastructure AddPlatformInfrastructure(
        this IDistributedApplicationBuilder builder)
    {
#pragma warning disable ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
        var redis = builder.AddRedis("infra-redis")
            .WithPassword(null)
            .WithEndpointProxySupport(false)
            .WithImage("redis", "latest")
            .WithDataVolume()
            .WithoutHttpsCertificate()
            .WithRedisInsight();
#pragma warning restore ASPIRECERTIFICATES001

        var redisTcpEndpoint = redis.GetEndpoint("tcp");

        // StackExchange.Redis expects "host:port" with no scheme.
        var dotNetRedisConnectionString = ReferenceExpression.Create(
            $"{redisTcpEndpoint.Property(EndpointProperty.Host)}:{redisTcpEndpoint.Property(EndpointProperty.Port)}");

        // Node clients expect a redis:// URI.
        var nodeRedisConnectionString = ReferenceExpression.Create(
            $"redis://{redisTcpEndpoint.Property(EndpointProperty.Host)}:{redisTcpEndpoint.Property(EndpointProperty.Port)}");

        var frontend = builder.AddDockerfile("infra-frontend", "../../", "docker/frontend/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, name: "http");

        return new PlatformInfrastructure(
            Redis: redis,
            DotNetRedisConnectionString: dotNetRedisConnectionString,
            NodeRedisConnectionString: nodeRedisConnectionString,
            Frontend: frontend,
            FrontendEndpoint: frontend.GetEndpoint("http"));
    }
}
