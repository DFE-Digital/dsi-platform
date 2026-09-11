using System.Text.Json;

namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Services;

/// <summary>
/// 
/// </summary>
/// <param name="configuration"></param>
public sealed class GenericEmailCheck(IConfiguration configuration)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public bool IsEmailGeneric(string email)
    {
        const string GenericEmailStrings = "GENERIC_EMAIL_STRINGS";
        var json = configuration.GetValue<string>(GenericEmailStrings);
        var genericEmailStrings =
        JsonSerializer.Deserialize<List<string>>(json ?? "[]") ?? [];

        return genericEmailStrings.Contains(email);

    }
}
