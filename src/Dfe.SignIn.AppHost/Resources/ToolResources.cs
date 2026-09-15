namespace Dfe.SignIn.AppHost.Resources;

/// <summary>
/// Registers local development tooling resources.
/// </summary>
public static class ToolResources
{
    /// <summary>
    /// Adds the TLS proxy executable when enabled.
    /// </summary>
    public static void AddTlsProxy(this IDistributedApplicationBuilder builder)
    {
        builder.AddExecutable("tool-tls-proxy", "pwsh", "../../", "-Command", "Start-DsiTlsProxy");
    }
}
