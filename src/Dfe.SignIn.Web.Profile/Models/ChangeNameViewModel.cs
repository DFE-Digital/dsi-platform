using Dfe.SignIn.Core.Contracts;
using FluentValidation;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their name.
/// </summary>
public sealed class ChangeNameViewModel
{
    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public string? FirstNameInput { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public string? LastNameInput { get; set; }
}

public sealed class ChangeNameViewModelValidator : AbstractValidator<ChangeNameViewModel>
{
    /// <inheritdoc />
    public ChangeNameViewModelValidator()
    {
        this.RuleFor(x => x.FirstNameInput)
            .NotEmpty().WithMessage("Enter a first name")
            .MaximumLength(60).WithMessage("Enter a name with no more than 60 characters")
            .Matches(StringPatterns.FirstNamePattern).WithMessage("Special characters cannot be used in first name");

        this.RuleFor(x => x.LastNameInput)
            .NotEmpty().WithMessage("Enter a last name")
            .MaximumLength(60).WithMessage("Enter a name with no more than 60 characters")
            .Matches(StringPatterns.LastNamePattern).WithMessage("Special characters cannot be used in last name");
    }
}
