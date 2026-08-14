using Dfe.SignIn.TestHelpers.Integration.Mocks;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public sealed record IntegrationTestContext
{
    /// <summary>
    /// The HttpClient instance to use for making requests to the internal API.
    /// </summary>
    public required HttpClient Client { get; init; }

    /// <summary>
    /// Populated if .WithAuditMock() was called on the builder.
    /// </summary>
    public CapturingWriteToAuditInteractor? AuditMock { get; init; }

    /// <summary>
    /// Direct access to the email request tracker for verifying notifications.
    /// </summary>
    public FakeEmailRequestTracker? EmailTracker { get; init; }
}
