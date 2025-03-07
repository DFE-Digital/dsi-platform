using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Options for the "select organisation" feature.
/// </summary>
public sealed class SelectOrganisationOptions : IOptions<SelectOrganisationOptions>
{
    /// <summary>
    /// Gets or sets the base address of the "select organisation" web frontend.
    /// </summary>
    public Uri SelectOrganisationBaseAddress { get; set; } = new Uri("http://localhost:5054");

    /// <summary>
    /// Gets or sets the session timeout duration in minutes.
    /// </summary>
    public int SessionTimeoutInMinutes { get; set; } = 10;

    /// <inheritdoc/>
    SelectOrganisationOptions IOptions<SelectOrganisationOptions>.Value => this;
}
