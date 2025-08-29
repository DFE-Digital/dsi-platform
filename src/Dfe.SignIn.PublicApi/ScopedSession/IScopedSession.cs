
using Dfe.SignIn.Core.InternalModels.Applications;

namespace Dfe.SignIn.PublicApi.ScopedSession;

/// <summary>
/// ScopedSessionReader
/// </summary>
public interface IScopedSessionReader
{
    /// <summary>
    /// Getter for Application
    /// </summary>
    ApplicationModel Application { get; }
}

/// <summary>
/// ScopedSessionWriter
/// </summary>
public interface IScopedSessionWriter
{
    /// <summary>
    /// Setter for Application
    /// </summary>
    ApplicationModel Application { set; }
}
