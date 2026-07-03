using Dfe.SignIn.Core.Contracts.Features.Applications.GetApplicationByClientId;
using FluentValidation;

namespace Dfe.SignIn.InternalApi.Feature.Applications.GetApplicationByClientId;

/// <summary>
/// Validator for the <see cref="GetApplicationByClientIdRequest"/> class.
/// </summary>
public sealed class GetApplicationByClientIdValidator : AbstractValidator<GetApplicationByClientIdRequest>
{
    /// <inheritdoc />
    public GetApplicationByClientIdValidator()
    {
        this.RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required.");
    }
}
