using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.ExternalModels.UnitTests.SelectOrganisation;

[TestClass]
public sealed class ExternalModelsJsonExtensionsTests
{
    #region ConfigureExternalModelJsonSerialization(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConfigureExternalModelJsonSerialization_Throws_WhenServicesArgumentIsNull()
    {
        ExternalModelsJsonExtensions.ConfigureExternalModelJsonSerialization(null!);
    }

    [TestMethod]
    public void ConfigureExternalModelJsonSerialization_ReturnsServices()
    {
        var services = new ServiceCollection();

        var result = services.ConfigureExternalModelJsonSerialization();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void ConfigureExternalModelJsonSerialization_RegistersSelectOrganisationExtensions()
    {
        var services = new ServiceCollection();

        services.ConfigureExternalModelJsonSerialization();

        var provider = services.BuildServiceProvider();
        var optionsAccessor = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>();
        var options = optionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);

        Assert.IsTrue(
            options.Converters.Any(converter => converter.Type == typeof(SelectOrganisationCallbackSelection))
        );
    }

    #endregion
}
