using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using FluentValidation;

namespace Dfe.SignIn.InternalApi.Feature.Users.GetUserProfile;

/// <summary>
/// Validator for the <see cref="GetUserProfileRequestA"/> class.
/// </summary>
public sealed class GetUserProfileRequestValidator : AbstractValidator<GetUserProfileRequestA>
{
    /// <inheritdoc />
    public GetUserProfileRequestValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
