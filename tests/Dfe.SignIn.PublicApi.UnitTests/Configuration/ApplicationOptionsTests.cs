using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class ApplicationOptionsTests
{
    #region IOptions<ApplicationOptions>.Value

    [TestMethod]
    public void IOptions_ApplicationOptions_Value_ReturnsThis()
    {
        var options = new ApplicationOptions();

        var value = (options as IOptions<ApplicationOptions>).Value;

        Assert.AreSame(options, value);
    }

    #endregion
}
