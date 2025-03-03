namespace Dfe.SignIn.NodeApiClient;

/// <summary>
/// Specifies the name of a Node.js mid-tier API.
/// </summary>
public enum NodeApiName
{
    /// <summary>
    /// Identifies the <c>login.dfe.access</c> mid-tier API.
    /// </summary>
    Access,

    /// <summary>
    /// Identifies the <c>login.dfe.applications</c> mid-tier API.
    /// </summary>
    Applications,

    /// <summary>
    /// Identifies the <c>login.dfe.directories</c> mid-tier API.
    /// </summary>
    Directories,

    /// <summary>
    /// Identifies the <c>login.dfe.organisations</c> mid-tier API.
    /// </summary>
    Organisations,

    /// <summary>
    /// Identifies the <c>login.dfe.search</c> mid-tier API.
    /// </summary>
    Search,
}
