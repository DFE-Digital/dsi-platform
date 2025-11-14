namespace Dfe.SignIn.Core.Interfaces.Audit;

/// <summary>
/// The names of headers that are used when raising audit events.
/// </summary>
public static class AuditHeaderNames
{
    /// <summary>
    /// The name of the header that specifies the name of the application that initiated
    /// the request. This is useful where a middle-tier API is sending an audit event as
    /// a result of a user interaction in a frontend application.
    /// </summary>
    public const string SourceApplicationName = "X-Source-App";

    /// <summary>
    /// The name of the header that specifies the remote IP address that initiated the
    /// request. This is useful where a middle-tier API is sending an audit event as a
    /// result of a user interaction in a frontend application.
    /// </summary>
    public const string SourceIpAddress = "X-Source-Ip";

    /// <summary>
    /// The name of the header that specifies the ID of the user that initiated the
    /// request. This is useful where a middle-tier API is sending an audit event as a
    /// result of a user interaction in a frontend application.
    /// </summary>
    public const string SourceUserId = "X-Source-User";
}
