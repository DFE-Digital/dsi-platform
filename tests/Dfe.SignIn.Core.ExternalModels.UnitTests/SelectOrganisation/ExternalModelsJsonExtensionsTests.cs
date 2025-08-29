using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.ExternalModels.UnitTests.SelectOrganisation;

[TestClass]
[SuppressMessage("csharpsquid", "S125",
    Justification = "Commented out code provides an example of registering JSON converters."
)]
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

    // [TestMethod]
    // public void ConfigureExternalModelJsonSerialization_RegistersExampleExtensions()
    // {
    //     var services = new ServiceCollection();

    //     services.ConfigureExternalModelJsonSerialization();

    //     var provider = services.BuildServiceProvider();
    //     var optionsAccessor = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>();
    //     var options = optionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);

    //     Assert.IsTrue(
    //         options.Converters.Any(converter => converter.Type == typeof(ExampleJsonConverter))
    //     );
    // }

    #endregion
}
