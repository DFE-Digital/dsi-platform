using Aspire.Hosting.Dotnet;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost.UnitTests;

[TestClass]
public sealed class FrontendResourceBuilderExtensionsTests
{
    [TestMethod]
    [DataRow("true")]
    [DataRow("false")]
    [DataRow(null)]
    public async Task WithFrontendAssets_ConfiguresProjectAddressVersionAndDependency(string? useHostedAssets)
    {
        var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions {
            Args = [],
            DisableDashboard = true
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
            ["Components:Frontend:UseHostedAssets"] = useHostedAssets,
            ["Assets:BaseAddress"] = "https://assets.example",
            ["Assets:FrontendVersion"] = "12.0.10"
        });
        var frontend = builder.AddContainer("frontend", "nginx")
            .WithHttpEndpoint(targetPort: 8080, name: "http");
        var endpoint = frontend.Resource.Annotations.OfType<EndpointAnnotation>().Single();
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 8080);
        var project = builder.AddDotnetProject("project", Path.Combine(Path.GetTempPath(), "project.csproj"),
            options => options.ExcludeLaunchProfile = true);

        var result = project.WithFrontendAssets(frontend, builder.Configuration);

        Assert.AreSame(project, result);
        var environment = new Dictionary<string, object>();
        var context = new EnvironmentCallbackContext(builder.ExecutionContext, project.Resource, environment);
        foreach (var callback in project.Resource.Annotations.OfType<EnvironmentCallbackAnnotation>()) {
            await callback.Callback(context);
        }

        var value = environment["Assets__BaseAddress"];
        var address = value is IValueProvider provider ? await provider.GetValueAsync(default) : value.ToString();
        Assert.AreEqual(useHostedAssets == "true" ? "https://assets.example" : "http://localhost:8080", address);
        Assert.AreEqual("12.0.10", environment["Assets__FrontendVersion"]);
        Assert.IsFalse(environment.ContainsKey("CDN_BASE_ADDRESS"));
        Assert.IsFalse(environment.ContainsKey("CDN_HOST_NAME"));
        var dependency = project.Resource.Annotations.OfType<WaitAnnotation>()
            .Single(annotation => ReferenceEquals(annotation.Resource, frontend.Resource));
        Assert.AreSame(frontend.Resource, dependency.Resource);
        Assert.AreEqual(WaitType.WaitUntilHealthy, dependency.WaitType);
    }

    [TestMethod]
    public void WithFrontendAssets_PreservesProjectBehaviorWhenHostedAddressIsMissing()
    {
        var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions {
            Args = [],
            DisableDashboard = true
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
            ["Components:Frontend:UseHostedAssets"] = "true",
            ["Assets:BaseAddress"] = null
        });
        var frontend = builder.AddContainer("frontend", "nginx");
        var project = builder.AddDotnetProject("project", Path.Combine(Path.GetTempPath(), "project.csproj"),
            options => options.ExcludeLaunchProfile = true);

        Assert.AreSame(project, project.WithFrontendAssets(frontend, builder.Configuration));
    }
}
