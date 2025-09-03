using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// Extension methods for <see cref="AutoMocker"/> instances.
/// </summary>
public static class AutoMockerExtensions
{
    #region JsonSerializerOptions

    /// <summary>
    /// Setup and use <see cref="JsonHelperExtensions.StandardOptionsKey"/> <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <returns>
    ///   <para>The standard <see cref="JsonSerializerOptions"/> instance which can be
    ///   further customised with additional converters as needed.</para>
    /// </returns>
    public static JsonSerializerOptions UseStandardJsonSerializerOptions(this AutoMocker autoMocker)
    {
        var standardOptions = JsonHelperExtensions.CreateStandardOptionsTestHelper();

        autoMocker.GetMock<IOptionsMonitor<JsonSerializerOptions>>()
            .Setup(mock => mock.Get(It.Is<string>(key => key == JsonHelperExtensions.StandardOptionsKey)))
            .Returns(standardOptions);

        return standardOptions;
    }

    #endregion
}
