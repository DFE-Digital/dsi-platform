using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class JsonHelperExtensionsTests
{
    #region SetupDfeSignInJsonSerializerOptions(IServiceCollection)

    [TestMethod]
    public void SetupDfeSignInJsonSerializerOptions_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => JsonHelperExtensions.ConfigureDfeSignInJsonSerializerOptions(null!));
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
