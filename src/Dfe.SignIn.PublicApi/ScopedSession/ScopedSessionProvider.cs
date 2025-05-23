using Dfe.SignIn.Core.InternalModels.Applications;

namespace Dfe.SignIn.PublicApi.ScopedSession;

/// <summary>
/// Scoped session provider implementation 
/// </summary>
public sealed class ScopedSessionProvider : IScopedSessionReader, IScopedSessionWriter
{

    /// <inheritdoc/>
    public ApplicationModel Application { get; set; } = default!;
}
