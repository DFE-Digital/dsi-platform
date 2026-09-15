namespace Dfe.SignIn.AppHost;

/// <summary>
/// Feature toggles controlling which AppHost components are started.
/// </summary>
public sealed class ComponentSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Components";

    /// <summary>
    /// Toggles for .NET project resources.
    /// </summary>
    public DotNetComponentSettings DotNet { get; set; } = new();

    /// <summary>
    /// Toggles for Node.js platform resources.
    /// </summary>
    public NodeComponentSettings Node { get; set; } = new();

    /// <summary>
    /// Toggles for local development tooling.
    /// </summary>
    public ToolComponentSettings Tools { get; set; } = new();
}

/// <summary>
/// Toggles for .NET Aspire project resources.
/// </summary>
public sealed class DotNetComponentSettings
{
    /// <summary>
    /// When true, starts the Help web app.
    /// </summary>
    public bool Help { get; set; } = true;

    /// <summary>
    /// When true, starts the Profile web app.
    /// </summary>
    public bool Profile { get; set; } = true;

    /// <summary>
    /// When true, starts the Public API.
    /// </summary>
    public bool PublicApi { get; set; } = true;
}

/// <summary>
/// Toggles for Node.js platform resources.
/// </summary>
public sealed class NodeComponentSettings
{
    /// <summary>
    /// When true, starts the OIDC Node app.
    /// </summary>
    public bool Oidc { get; set; } = false;

    /// <summary>
    /// When true, starts the Interactions Node app.
    /// </summary>
    public bool Interactions { get; set; } = false;

    /// <summary>
    /// When true, starts the Services Node app.
    /// </summary>
    public bool Services { get; set; } = false;
}

/// <summary>
/// Toggles for local development tooling resources.
/// </summary>
public sealed class ToolComponentSettings
{
    /// <summary>
    /// When true, starts the local TLS proxy tool.
    /// </summary>
    public bool TlsProxy { get; set; }
}
