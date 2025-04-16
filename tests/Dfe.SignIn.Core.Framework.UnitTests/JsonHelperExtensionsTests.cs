using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class JsonHelperExtensionsTests
{
    #region SetupDfeSignInJsonSerializerOptions(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupDfeSignInJsonSerializerOptions_Throws_WhenServicesArgumentIsNull()
    {
        JsonHelperExtensions.ConfigureDfeSignInJsonSerializerOptions(null!);
    }

    [TestMethod]
    public void CreateStandardOptions_UsesCamelCasing()
    {
        var options = JsonHelperExtensions.CreateStandardOptionsTestHelper();

        Assert.IsTrue(options.PropertyNameCaseInsensitive);
        Assert.AreEqual(JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
    }

    [TestMethod]
    public void CreateStandardOptions_IgnoresNullProperties()
    {
        var options = JsonHelperExtensions.CreateStandardOptionsTestHelper();

        Assert.AreEqual(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
    }

    [TestMethod]
    public void CreateStandardOptions_IgnoresUnmappedMembers()
    {
        var options = JsonHelperExtensions.CreateStandardOptionsTestHelper();

        Assert.AreEqual(JsonUnmappedMemberHandling.Skip, options.UnmappedMemberHandling);
    }

    #endregion
}
