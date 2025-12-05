using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Web.Profile.Services;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that prompts the user to select their external account.
/// </summary>
public sealed class SelectAssociatedAccountViewModel
{
    /// <summary>
    /// Gets or sets the location that the user will be returned to upon selecting
    /// their external account.
    /// </summary>
    [EnumDataType(typeof(SelectAssociatedReturnLocation))]
    public SelectAssociatedReturnLocation ReturnLocation { get; set; }
        = SelectAssociatedReturnLocation.Home;
}
