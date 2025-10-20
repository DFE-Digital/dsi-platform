using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their job title.
/// </summary>
public sealed class ChangeJobTitleViewModel
{
    /// <summary>
    /// Gets or sets the job title of the user.
    /// </summary>
    [MapTo<ChangeJobTitleRequest>(nameof(ChangeJobTitleRequest.NewJobTitle))]
    public string? JobTitleInput { get; set; }
}
