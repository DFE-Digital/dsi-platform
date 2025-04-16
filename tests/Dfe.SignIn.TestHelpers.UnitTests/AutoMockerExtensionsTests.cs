using System.Text.Json;
using Dfe.SignIn.Core.Framework;
using Microsoft.Extensions.Options;
using Moq.AutoMock;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class AutoMockerExtensionsTests
{
    #region UseStandardJsonSerializerOptions(AutoMocker)

    [TestMethod]
    public void UseStandardJsonSerializerOptions_AddsExpectedOptions()
    {
        var autoMocker = new AutoMocker();

        var expectedOptions = autoMocker.UseStandardJsonSerializerOptions();

        var optionsAccessor = autoMocker.Get<IOptionsMonitor<JsonSerializerOptions>>();
        var options = optionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
        Assert.AreSame(expectedOptions, options);
    }

    #endregion
}
