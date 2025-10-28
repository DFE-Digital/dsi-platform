using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Options for referencing frontend assets; for example, the design system .css and .js files.
/// </summary>
[SuppressMessage("csharpsquid", "S1075",
    Justification = "Default URLs configuration for running locally on a development machine."
)]
public sealed class AssetOptions : IOptions<AssetOptions>
{
    /// <summary>
    /// Gets the assets base URL.
    /// </summary>
    /// <remarks>
    ///   <para>Defaults to a URL that is suitable for a local development environment
    ///   when using the <c>login.dfe.ui-toolkit</c> package.</para>
    /// </remarks>
    public Uri BaseAddress { get; set; } = new Uri("http://localhost:8081");

    /// <summary>
    /// Gets the semantic version number of the frontend assets; eg. "1.2.3".
    /// </summary>
    public required string FrontendVersion { get; set; }

    /// <summary>
    /// Gets base address of assets including the resolved version number.
    /// </summary>
    public Uri VersionedBaseAddress => !string.IsNullOrEmpty(this.FrontendVersion)
        ? new(this.BaseAddress, $"./{this.FrontendVersion}/")
        : this.BaseAddress;

    /// <inheritdoc/>
    AssetOptions IOptions<AssetOptions>.Value => this;
}
