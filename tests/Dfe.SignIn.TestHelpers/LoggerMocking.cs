using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// Helper functionality for mocking  <see cref="ILogger{T}"/> to capture log entries.
/// </summary>
public static class LoggerMocking
{
    /// <summary>
    /// Get mocked <see cref="ILogger{T}"/> and capture log entries.
    /// </summary>
    /// <typeparam name="T">The type of logger.</typeparam>
    /// <param name="captureLog">Delegate to capture a log entry.</param>
    /// <returns>
    ///   <para>The mocked logger instance.</para>
    /// </returns>
    public static Mock<ILogger<T>> GetMockToCaptureLogs<T>(Action<string> captureLog)
    {
        var mockLogger = new Mock<ILogger<T>>();
        mockLogger
            .Setup(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            )
            .Callback((LogLevel level, EventId eventId, object state, Exception? exception, Delegate formatter)
                => captureLog($"{level}: {state}"));
        return mockLogger;
    }
}
