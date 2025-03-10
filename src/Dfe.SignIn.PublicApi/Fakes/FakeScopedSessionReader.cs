using Dfe.SignIn.Core.Models.Applications;
using Dfe.SignIn.PublicApi.ScopedSession;

/// <summary>
/// ...
/// </summary>
public sealed class FakeScopedSessionReader : IScopedSessionReader
{
    /// <inheritdoc/>
    public ApplicationModel Application => new() {
        Id = Guid.Empty,
        ClientId = "fake-client",
        Name = "Fake application",
        IsExternalService = false,
        IsHiddenService = false,
        IsIdOnlyService = false,
        ServiceHomeUrl = new Uri("https://fake-service.localhost"),
    };
}
