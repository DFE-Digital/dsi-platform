using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Dfe.SignIn.AppHost;

/// <summary>
/// Provides the platform Redis resource and connection-string formats.
/// </summary>
public static class RedisResourceBuilderExtensions
{
    /// <summary>
    /// Adds persistent local Redis with Redis Insight and infrastructure tags.
    /// </summary>
    public static IResourceBuilder<RedisResource> AddPlatformRedis(
        this IDistributedApplicationBuilder builder,
        string name)
    {
#pragma warning disable ASPIRERESOURCETRAITS001 // Dashboard tags are experimental in Aspire 17.
#pragma warning disable ASPIRECERTIFICATES001 // Certificate configuration is experimental in Aspire 17.
        return builder.AddRedis(name)
            .WithTags(ResourceTags.Infrastructure)
            .WithPassword(null)
            .WithEndpointProxySupport(false)
            .WithImage("redis", "latest")
            .WithDataVolume()
            .WithoutHttpsCertificate()
            .WithRedisInsight(insight => insight.WithTags(ResourceTags.Infrastructure));
#pragma warning restore ASPIRECERTIFICATES001
#pragma warning restore ASPIRERESOURCETRAITS001
    }

    /// <summary>
    /// Gets the host:port reference expected by StackExchange.Redis.
    /// </summary>
    public static ReferenceExpression GetDotnetConnectionString(this IResourceBuilder<RedisResource> builder)
    {
        var endpoint = builder.GetEndpoint("tcp");
        return ReferenceExpression.Create(
            $"{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}");
    }

    /// <summary>
    /// Gets the redis:// URI reference expected by the Node applications.
    /// </summary>
    public static ReferenceExpression GetNodeConnectionString(this IResourceBuilder<RedisResource> builder)
    {
        var endpoint = builder.GetEndpoint("tcp");
        return ReferenceExpression.Create(
            $"redis://{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}");
    }
}
