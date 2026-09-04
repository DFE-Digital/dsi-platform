using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using FluentValidation;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their email address.
/// </summary>
public sealed class ChangeEmailViewModel
{
    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public required string EmailAddressInput { get; set; }
}

public sealed class ChangeEmailViewModelValidator : AbstractValidator<ChangeEmailViewModel>
{
    /// <inheritdoc />
    public ChangeEmailViewModelValidator()
    {
        this.RuleFor(x => x.EmailAddressInput)
            .NotEmpty().WithMessage("Enter an email address")
            .MaximumLength(UserConstants.MaxEmailAddressLength).WithMessage($"Enter an email address with no more than {UserConstants.MaxEmailAddressLength} characters")
            .EmailAddress().WithMessage("Enter a valid email address");
    }
}
