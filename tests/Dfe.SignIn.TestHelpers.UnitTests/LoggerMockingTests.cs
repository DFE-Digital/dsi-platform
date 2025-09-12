using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class LoggerMockingTests
{
    #region GetMockToCaptureLogs(Action<string>)

    [TestMethod]
    public void GetMockToCaptureLogs_ReturnsMockThatCapturesLogEntries()
    {
        var capturedLogs = new List<string>();
        var mock = LoggerMocking.GetMockToCaptureLogs<LoggerMockingTests>(capturedLogs.Add);

        mock.Object.LogInformation("Example information.");
        mock.Object.LogError("An example error message.");

        string[] expectedLogs = [
            "Information: Example information.",
            "Error: An example error message.",
        ];
        CollectionAssert.AreEqual(expectedLogs, capturedLogs);
    }

    #endregion
}
