using System.Text.Json;

namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Services;

/// <summary>
/// An implementation of the generic email checker.
///
/// Designed to validate the provided email against a pre-configured
/// generic blacklist.
/// </summary>
/// <param name="configuration"></param>
public sealed class GenericEmailCheck(IConfiguration configuration)
{
    /// <summary>
    /// A check to determine if an email address is generic.
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <returns>True, if the value is considered generic</returns>
    public bool IsEmailGeneric(string email)
    {
        const string GenericEmailStrings = "GENERIC_EMAIL_STRINGS";
        var json = configuration.GetValue<string>(GenericEmailStrings);
        var genericEmailStrings =
        JsonSerializer.Deserialize<List<string>>(json ?? "[]") ?? [];

        return genericEmailStrings.Contains(email);

    }
}
