using Dfe.SignIn.InternalApi.Configuration;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Services;

/// <summary>
/// An implementation of the generic email checker.
///
/// Designed to validate the provided email against a pre-configured
/// generic blacklist.
/// </summary>
/// <param name="emailRestrictions"></param>
public sealed class GenericEmailCheck(IOptions<EmailRestrictionsSettings> emailRestrictions)
{
    /// <summary>
    /// A check to determine if an email address is generic.
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <returns>True, if the value is considered generic</returns>
    public bool IsEmailGeneric(string email)
    {
        return emailRestrictions.Value.GenericEmailStrings.Contains(email);

    }
}
