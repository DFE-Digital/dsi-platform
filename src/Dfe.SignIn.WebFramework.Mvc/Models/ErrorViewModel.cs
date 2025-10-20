namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// View model for a general purpose error page.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Gets a unique identifier representing the request.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the request ID should be presented.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
}
