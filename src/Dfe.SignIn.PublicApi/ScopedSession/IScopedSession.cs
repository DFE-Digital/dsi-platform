
using Dfe.SignIn.Core.Contracts.Applications;

namespace Dfe.SignIn.PublicApi.ScopedSession;

/// <summary>
/// ScopedSessionReader
/// </summary>
public interface IScopedSessionReader
{
    /// <summary>
    /// Getter for Application
    /// </summary>
    Application Application { get; }
}

/// <summary>
/// ScopedSessionWriter
/// </summary>
public interface IScopedSessionWriter
{
    /// <summary>
    /// Setter for Application
    /// </summary>
    Application Application { set; }
}
