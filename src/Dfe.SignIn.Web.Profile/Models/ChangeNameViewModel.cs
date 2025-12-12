using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their name.
/// </summary>
public sealed class ChangeNameViewModel
{
    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    [MapTo<ChangeNameRequest>(nameof(ChangeNameRequest.FirstName))]
    public string? FirstNameInput { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    [MapTo<ChangeNameRequest>(nameof(ChangeNameRequest.LastName))]
    public string? LastNameInput { get; set; }
}
