using Dfe.SignIn.WebFramework.Configuration;

namespace Dfe.SignIn.WebFramework.UnitTests.Configuration;

[TestClass]
public sealed class AssetOptionsTests
{
    #region VersionedBaseAddress

    [TestMethod]
    public void VersionedBaseAddress_ReturnsExpectedValue_WhenVersionPresent()
    {
        var options = new AssetOptions {
            BaseAddress = new Uri("http://cdn.localhost/base/"),
            FrontendVersion = "1.2.3",
        };

        Assert.AreEqual(new Uri("http://cdn.localhost/base/1.2.3/"), options.VersionedBaseAddress);
    }

    [TestMethod]
    public void VersionedBaseAddress_ReturnsExpectedValue_WhenNoVersionPresent()
    {
        var options = new AssetOptions {
            BaseAddress = new Uri("http://cdn.localhost/base/"),
            FrontendVersion = "",
        };

        Assert.AreEqual(new Uri("http://cdn.localhost/base/"), options.VersionedBaseAddress);
    }

    #endregion
}
