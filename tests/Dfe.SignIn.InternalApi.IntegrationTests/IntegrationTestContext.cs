using Dfe.SignIn.TestHelpers.Integration.Mocks;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public sealed record IntegrationTestContext : IDisposable
{
    /// <summary>
    /// The HttpClient instance to use for making requests to the internal API.
    /// </summary>
    public required HttpClient Client { get; init; }

    public required CapturingWriteToAuditInteractor AuditMock { get; init; }

    public required FakeEmailRequestTracker EmailTracker { get; init; }

    /// <summary>
    /// The DI container backing this client, reflecting any WithX(...) overrides applied.
    /// </summary>
    public required IServiceProvider Services { get; init; }

    public void Dispose() => this.Client.Dispose();
}
