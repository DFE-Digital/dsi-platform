using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost.UnitTests;

[TestClass]
public sealed class NodeResourceBuilderExtensionsTests
{
    [TestMethod]
    [DataRow("true")]
    [DataRow("false")]
    [DataRow(null)]
    public async Task WithFrontendAssets_SelectsAddressAndOverridesEnvFileDefaults(string? useHostedAssets)
    {
        var envFilePath = Path.GetTempFileName();
        try {
            await File.WriteAllLinesAsync(envFilePath, [
                "Assets__BaseAddress=https://env-file.example",
                "CDN_BASE_ADDRESS=https://env-file.example",
                "CDN_HOST_NAME=https://env-file.example",
                "UNCHANGED_SETTING=preserved"
            ]);

            var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions {
                Args = [],
                DisableDashboard = true
            });
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
                ["Components:Frontend:UseHostedAssets"] = useHostedAssets,
                ["Assets:BaseAddress"] = "https://assets.example"
            });

            var frontend = builder.AddContainer("frontend", "nginx")
                .WithHttpEndpoint(targetPort: 8080, name: "http");
            var endpoint = frontend.Resource.Annotations.OfType<EndpointAnnotation>().Single();
            endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 8080);

            var node = builder.AddNodePlatformApp(
                "node",
                Path.GetDirectoryName(envFilePath)!,
                ".",
                4436,
                envFileName: Path.GetFileName(envFilePath));

            var result = node.WithFrontendAssets(frontend, builder.Configuration);

            Assert.AreSame(node, result);
            var environment = new Dictionary<string, object>();
            var context = new EnvironmentCallbackContext(builder.ExecutionContext, node.Resource, environment);
            foreach (var callback in node.Resource.Annotations.OfType<EnvironmentCallbackAnnotation>()) {
                await callback.Callback(context);
            }

            var expectedAddress = useHostedAssets == "true" ? "https://assets.example" : "http://localhost:8080";
            foreach (var key in new[] { "Assets__BaseAddress", "CDN_BASE_ADDRESS", "CDN_HOST_NAME" }) {
                var value = environment[key];
                var address = value is IValueProvider provider ? await provider.GetValueAsync(default) : value.ToString();
                Assert.AreEqual(expectedAddress, address, key);
            }

            Assert.AreEqual("preserved", environment["UNCHANGED_SETTING"]);
            var dependency = node.Resource.Annotations.OfType<WaitAnnotation>()
                .Single(annotation => ReferenceEquals(annotation.Resource, frontend.Resource));
            Assert.AreSame(frontend.Resource, dependency.Resource);
            Assert.AreEqual(WaitType.WaitUntilHealthy, dependency.WaitType);
        }
        finally {
            File.Delete(envFilePath);
        }
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void WithFrontendAssets_ThrowsWhenHostedAddressIsMissing(string? baseAddress)
    {
        var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions {
            Args = [],
            DisableDashboard = true
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
            ["Components:Frontend:UseHostedAssets"] = "true",
            ["Assets:BaseAddress"] = baseAddress
        });
        var frontend = builder.AddContainer("frontend", "nginx").WithHttpEndpoint(targetPort: 8080);
        var node = builder.AddJavaScriptApp("node", Path.GetTempPath()).WithNpm();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() =>
            node.WithFrontendAssets(frontend, builder.Configuration));

        StringAssert.Contains(exception.Message, "Assets:BaseAddress");
    }
}
