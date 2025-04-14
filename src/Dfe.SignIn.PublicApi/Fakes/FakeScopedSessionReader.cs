using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.InternalModels.Applications;
using Dfe.SignIn.PublicApi.ScopedSession;

namespace Dfe.SignIn.PublicApi.Fakes;

/// <summary>
/// ...
/// </summary>
[ExcludeFromCodeCoverage]
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
