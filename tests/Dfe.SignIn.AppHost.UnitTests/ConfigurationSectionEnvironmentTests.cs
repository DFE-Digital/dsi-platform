using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost.UnitTests;

[TestClass]
public sealed class ConfigurationSectionEnvironmentTests
{
    [TestMethod]
    public void EnumerateSectionAsEnvironmentVariables_ConvertsColonsAndSkipsNulls()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EntityFramework:Directories:Host"] = "sql.local",
                ["EntityFramework:Directories:Name"] = "dirs",
                ["EntityFramework:Organisations:Host"] = "sql.local",
                ["Other:Section:Key"] = "ignored",
            })
            .Build();

        var values = ResourceBuilderExtensions
            .EnumerateSectionAsEnvironmentVariables(configuration, "EntityFramework")
            .OrderBy(pair => pair.Key)
            .ToList();

        CollectionAssert.AreEqual(
            new List<KeyValuePair<string, string>>
            {
                new("EntityFramework__Directories__Host", "sql.local"),
                new("EntityFramework__Directories__Name", "dirs"),
                new("EntityFramework__Organisations__Host", "sql.local"),
            },
            values);
    }

    [TestMethod]
    public void EnumerateSectionAsEnvironmentVariables_SupportsNestedSectionPaths()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["InternalApiClient:Access:BaseAddress"] = "https://access.local",
                ["InternalApiClient:Organisations:BaseAddress"] = "https://orgs.local",
                ["InternalApiClient:ClientId"] = "client",
            })
            .Build();

        var values = ResourceBuilderExtensions
            .EnumerateSectionAsEnvironmentVariables(configuration, "InternalApiClient:Access")
            .ToList();

        CollectionAssert.AreEqual(
            new List<KeyValuePair<string, string>>
            {
                new("InternalApiClient__Access__BaseAddress", "https://access.local"),
            },
            values);
    }
}
