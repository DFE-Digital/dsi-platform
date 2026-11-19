using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Dotnet;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost.UnitTests;

[TestClass]
public sealed class ProjectResourceBuilderExtensionsTests
{
    [TestMethod]
    [DataRow(null, "Local")]
    [DataRow("Staging", "Staging")]
    public async Task AddDotnetPlatformApp_UsesHttpProfileAndConfiguredOrDefaultEnvironment(string? configuredEnvironment, string expectedEnvironment)
    {
        var directory = Directory.CreateTempSubdirectory("dsi-apphost-tests-");
        try {
            var projectPath = Path.Combine(directory.FullName, "project.csproj");
            await File.WriteAllTextAsync(projectPath, """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
                </Project>
                """);
            var properties = directory.CreateSubdirectory("Properties");
            await File.WriteAllTextAsync(Path.Combine(properties.FullName, "launchSettings.json"), """
                {
                  "profiles": {
                    "https": {
                      "commandName": "Project",
                      "environmentVariables": { "SELECTED_PROFILE": "https" }
                    },
                    "http": {
                      "commandName": "Project",
                      "environmentVariables": {
                        "SELECTED_PROFILE": "http",
                        "ASPNETCORE_ENVIRONMENT": "Development"
                      }
                    }
                  }
                }
                """);
            var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions {
                Args = [],
                DisableDashboard = true
            });
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
                ["ASPNETCORE_ENVIRONMENT"] = configuredEnvironment
            });

            var project = builder.AddDotnetPlatformApp("project", projectPath);

            var environment = new Dictionary<string, object>();
            var context = new EnvironmentCallbackContext(builder.ExecutionContext, project.Resource, environment);
            foreach (var callback in project.Resource.Annotations.OfType<EnvironmentCallbackAnnotation>()) {
                await callback.Callback(context);
            }

            Assert.AreEqual("http", environment["SELECTED_PROFILE"]);
            Assert.AreEqual(expectedEnvironment, environment["ASPNETCORE_ENVIRONMENT"]);
        }
        finally {
            directory.Delete(recursive: true);
        }
    }

    [TestMethod]
    [DataRow("help", "Local")]
    [DataRow("internal-api", "Local")]
    [DataRow("internal-api", "Development")]
    [DataRow("internal-api", "local")]
    [DataRow("internal-api", null)]
    [DataRow("profile", "Local")]
    [DataRow("public-api", "Local")]
    public async Task WithApplicationSettings_PreservesEnvironmentMappings(string application, string? environmentName)
    {
        (string[] settings, string[] redisKeys) = application switch {
            "help" => (new[] {
                "GovNotify:ApiKey",
                "RaiseSupportTicketByEmail:SupportEmailAddress",
                "RaiseSupportTicketByEmail:EmailTemplateId"
            }, new[] { "InteractionsRedisCache__ConnectionString" }),
            "internal-api" => (new[] {
                "EntityFramework:Directories:Host", "EntityFramework:Directories:Name",
                "EntityFramework:Directories:Username", "EntityFramework:Directories:Password",
                "EntityFramework:Organisations:Host", "EntityFramework:Organisations:Name",
                "EntityFramework:Organisations:Username", "EntityFramework:Organisations:Password",
                "PublicApiSecretEncryption:Key",
                "ExternalId:ClientId", "ExternalId:ClientSecret", "ExternalId:TenantId",
                "GeneralRedisCache:DatabaseNumber", "GovNotify:ApiKey"
            }, new[] { "GeneralRedisCache__ConnectionString", "BullMQ__ConnectionString" }),
            "profile" => (new[] {
                "Oidc:ClientId", "Oidc:ClientSecret", "Oidc:Authority", "Oidc:MetadataAddress",
                "ExternalId:ClientId", "ExternalId:ClientSecret", "ExternalId:Authority",
                "ExternalId:Instance", "ExternalId:TenantId",
                "Session:DurationInMinutes", "Session:NotifyRemainingMinutes"
            }, new[] { "GeneralRedisCache__ConnectionString", "SessionRedisCache__ConnectionString", "TokenRedisCache__ConnectionString" }),
            "public-api" => (new[] {
                "BearerToken:ValidAudience", "PublicApiSecretEncryption:Key",
                "SelectOrganisation:SelectOrganisationBaseAddress",
                "EntityFramework:Organisations:Username", "EntityFramework:Organisations:Password",
                "EntityFramework:Organisations:Name", "EntityFramework:Organisations:Host",
                "EntityFramework:Directories:Username", "EntityFramework:Directories:Password",
                "EntityFramework:Directories:Name", "EntityFramework:Directories:Host",
                "EntityFramework:Audit:Username", "EntityFramework:Audit:Password",
                "EntityFramework:Audit:Name", "EntityFramework:Audit:Host"
            }, new[] { "SelectOrganisationSessionRedisCache__ConnectionString", "InteractionsRedisCache__ConnectionString" }),
            _ => throw new ArgumentOutOfRangeException(nameof(application))
        };
        var configurationValues = settings.ToDictionary(key => key, key => (string?)$"test-{key}");
        configurationValues["ASPNETCORE_ENVIRONMENT"] = environmentName;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationValues).Build();
        var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions {
            Args = [],
            DisableDashboard = true
        });
        var project = builder.AddDotnetProject(application, Path.Combine(Path.GetTempPath(), "project.csproj"),
            options => options.ExcludeLaunchProfile = true);
        var redisConnectionString = ReferenceExpression.Create($"localhost:6379");

        var result = application switch {
            "help" => project
                .WithRedisConnectionStrings(redisConnectionString, "InteractionsRedisCache")
                .WithGovNotifySettings(configuration)
                .WithSupportEmailSettings(configuration),
            "internal-api" => project
                .WithEnvironment("Authentication__LocalBypass", configuration["ASPNETCORE_ENVIRONMENT"] == "Local" ? "true" : "false")
                .WithEfSettings(configuration, "Directories", "Organisations")
                .WithPublicApiSecretEncryptionSettings(configuration)
                .WithExternalIdClientSettings(configuration)
                .WithRedisConnectionStrings(redisConnectionString, "GeneralRedisCache", "BullMQ")
                .WithEnvironment("GeneralRedisCache__DatabaseNumber", configuration["GeneralRedisCache:DatabaseNumber"])
                .WithGovNotifySettings(configuration),
            "profile" => project
                .WithRedisConnectionStrings(redisConnectionString, "GeneralRedisCache", "SessionRedisCache", "TokenRedisCache")
                .WithOidcSettings(configuration)
                .WithExternalIdSettings(configuration)
                .WithSessionSettings(configuration),
            "public-api" => project
                .WithRedisConnectionStrings(redisConnectionString, "SelectOrganisationSessionRedisCache", "InteractionsRedisCache")
                .WithBearerTokenSettings(configuration)
                .WithPublicApiSecretEncryptionSettings(configuration)
                .WithSelectOrganisationSettings(configuration)
                .WithEfSettings(configuration, "Organisations", "Directories", "Audit"),
            _ => throw new ArgumentOutOfRangeException(nameof(application))
        };

        Assert.AreSame(project, result);
        var environment = new Dictionary<string, object>();
        var context = new EnvironmentCallbackContext(builder.ExecutionContext, project.Resource, environment);
        foreach (var callback in project.Resource.Annotations.OfType<EnvironmentCallbackAnnotation>()) {
            await callback.Callback(context);
        }

        foreach (var key in settings) {
            Assert.AreEqual(configurationValues[key], environment[key.Replace(":", "__")], key);
        }

        foreach (var key in redisKeys) {
            Assert.AreSame(redisConnectionString, environment[key], key);
        }

        if (application == "internal-api") {
            Assert.AreEqual(environmentName == "Local" ? "true" : "false", environment["Authentication__LocalBypass"]);
            Assert.IsFalse(environment.ContainsKey("ExternalId__Authority"));
            Assert.IsFalse(environment.ContainsKey("EntityFramework__Audit__Host"));
        }

        if (application == "profile") {
            Assert.IsFalse(environment.ContainsKey("GeneralRedisCache__DatabaseNumber"));
        }
    }
}
