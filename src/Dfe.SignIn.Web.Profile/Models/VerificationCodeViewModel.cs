using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to input a verification code.
/// </summary>
public sealed class VerificationCodeViewModel
{
    /// <summary>
    /// The unique 'TempData' key to hide the resend email verification action.
    /// </summary>
    public const string HideResendVerificationTempDataKey = "HideResendVerification";

    /// <summary>
    /// Gets or sets the unique ID of the user.
    /// </summary>
    [ValidateNever]
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the new email address of the user.
    /// </summary>
    [ValidateNever]
    public required string NewEmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the user's verification code.
    /// </summary>
    public string? VerificationCodeInput { get; set; }

    /// <summary>
    /// Gets a mapping of the properties of this view model to the properties of a <see cref="ConfirmChangeEmailAddressRequest"/>.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> RequestPropertyMap = new Dictionary<string, string> {
        [nameof(ConfirmChangeEmailAddressRequest.VerificationCode)] = nameof(VerificationCodeInput),
    };
}

public sealed class VerificationCodeViewModelValidator : AbstractValidator<VerificationCodeViewModel>
{
    /// <inheritdoc />
    public VerificationCodeViewModelValidator()
    {
        this.RuleFor(x => x.VerificationCodeInput)
            .NotEmpty().WithMessage("Enter a verification code");
    }
}
