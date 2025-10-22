namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their job title.
/// </summary>
public sealed class ChangeJobTitleViewModel
{
    /// <summary>
    /// Gets or sets the job title of the user.
    /// </summary>
    public string? JobTitleInput { get; set; }
}
