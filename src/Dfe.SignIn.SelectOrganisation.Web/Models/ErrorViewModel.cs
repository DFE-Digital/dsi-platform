namespace Dfe.SignIn.SelectOrganisation.Web.Models;

/// <summary>
/// View model for a general purpose error page.
/// </summary>
public sealed record ErrorViewModel
{
    /// <summary>
    /// Gets a unique identifier representing the request.
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the request ID should be presented.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
}
