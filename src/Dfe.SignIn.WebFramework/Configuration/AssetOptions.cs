using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Options for referencing frontend assets; for example, the design system .css and .js files.
/// </summary>
public sealed class AssetOptions : IOptions<AssetOptions>
{
    /// <summary>
    /// Gets the assets base URL.
    /// </summary>
    /// <remarks>
    ///   <para>Defaults to a URL that is suitable for a local development environment
    ///   when using the <c>login.dfe.ui-toolkit</c> package.</para>
    /// </remarks>
    public Uri BaseAddress { get; set; } = new Uri("https://localhost:3001");

    /// <summary>
    /// Assets version string.
    /// </summary>
    /// <remarks>
    ///   <para>This is used to invalidate outdated versions of assets that have been
    ///   cached in the user's web browser.</para>
    /// </remarks>
    public string Version { get; set; } = "0";

    /// <inheritdoc/>
    AssetOptions IOptions<AssetOptions>.Value => this;
}
