using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Contracts;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their job title.
/// </summary>
public sealed class ChangeJobTitleViewModel
{
    /// <summary>
    /// Gets or sets the job title of the user.
    /// </summary>
    [Required(ErrorMessage = "Enter a job title")]
    [RegularExpression(StringPatterns.JobTitlePattern, ErrorMessage = "Special characters cannot be used in job title")]
    [MaxLength(60, ErrorMessage = "Enter a job title with no more than 60 characters")]
    public string? JobTitleInput { get; set; }
}
