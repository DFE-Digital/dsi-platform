using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.UnitTests.Configuration;

[TestClass]
public sealed class PlatformOptionsTests
{
    #region IOptions<PlatformOptions>.Value

    [TestMethod]
    public void IOptions_PlatformOptions_Value_ReturnsThis()
    {
        var options = new PlatformOptions();

        var value = (options as IOptions<PlatformOptions>).Value;

        Assert.AreSame(options, value);
    }

    #endregion
}
