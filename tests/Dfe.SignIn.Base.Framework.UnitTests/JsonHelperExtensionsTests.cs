using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class JsonHelperExtensionsTests
{
    #region CreateStandardOptionsTestHelper()

    [TestMethod]
    public void CreateStandardOptionsTestHelper_UsesCamelCasing()
    {
        var options = JsonHelperExtensions.CreateStandardOptionsTestHelper();

        Assert.IsTrue(options.PropertyNameCaseInsensitive);
        Assert.AreEqual(JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
    }

    [TestMethod]
    public void CreateStandardOptionsTestHelper_IgnoresNullProperties()
    {
        var options = JsonHelperExtensions.CreateStandardOptionsTestHelper();

        Assert.AreEqual(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
    }

    [TestMethod]
    public void CreateStandardOptionsTestHelper_IgnoresUnmappedMembers()
    {
        var options = JsonHelperExtensions.CreateStandardOptionsTestHelper();

        Assert.AreEqual(JsonUnmappedMemberHandling.Skip, options.UnmappedMemberHandling);
    }

    #endregion

    #region SetupDfeSignInJsonSerializerOptions(IServiceCollection)

    [TestMethod]
    public void SetupDfeSignInJsonSerializerOptions_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => JsonHelperExtensions.ConfigureDfeSignInJsonSerializerOptions(null!));
    }

    [TestMethod]
    public void SetupDfeSignInJsonSerializerOptions_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        JsonHelperExtensions.ConfigureDfeSignInJsonSerializerOptions(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IExceptionJsonSerializer) &&
                descriptor.ImplementationType == typeof(DefaultExceptionJsonSerializer)
            )
        );
    }

    #endregion
}
