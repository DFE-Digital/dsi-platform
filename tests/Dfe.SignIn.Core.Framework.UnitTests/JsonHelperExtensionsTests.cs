using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class JsonHelperExtensionsTests
{
    #region CreateStandardOptions()

    [TestMethod]
    public void CreateStandardOptions_UsesCamelCasing()
    {
        var options = JsonHelperExtensions.CreateStandardOptions();

        Assert.IsTrue(options.PropertyNameCaseInsensitive);
        Assert.AreEqual(JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
    }

    [TestMethod]
    public void CreateStandardOptions_IgnoresNullProperties()
    {
        var options = JsonHelperExtensions.CreateStandardOptions();

        Assert.AreEqual(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
    }

    [TestMethod]
    public void CreateStandardOptions_IgnoresUnmappedMembers()
    {
        var options = JsonHelperExtensions.CreateStandardOptions();

        Assert.AreEqual(JsonUnmappedMemberHandling.Skip, options.UnmappedMemberHandling);
    }

    #endregion

    #region SetupDfeSignInJsonSerializerOptions(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupDfeSignInJsonSerializerOptions_Throws_WhenServicesArgumentIsNull()
    {
        JsonHelperExtensions.SetupDfeSignInJsonSerializerOptions(null!);
    }

    [TestMethod]
    public void SetupDfeSignInJsonSerializerOptions_RegistersExpectedOptions()
    {
        var services = new ServiceCollection();

        services.SetupDfeSignInJsonSerializerOptions();

        Assert.IsTrue(
            services.Any(descriptor =>
                (string?)descriptor.ServiceKey == JsonHelperExtensions.StandardOptionsKey &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(JsonSerializerOptions)
            )
        );
    }

    #endregion
}
