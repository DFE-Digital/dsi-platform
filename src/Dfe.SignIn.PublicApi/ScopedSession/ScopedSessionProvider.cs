using Dfe.SignIn.Core.Contracts.Applications;

namespace Dfe.SignIn.PublicApi.ScopedSession;

/// <summary>
/// Scoped session provider implementation
/// </summary>
public sealed class ScopedSessionProvider : IScopedSessionReader, IScopedSessionWriter
{

    /// <inheritdoc/>
    public Application Application { get; set; } = default!;
}
