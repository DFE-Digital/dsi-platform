using System.Text.RegularExpressions;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

/// <summary>
/// Contains the generated regex for matching valid policy version codes.
/// </summary>
/// <remarks>
/// This class is declared as <c>partial</c> to enable the C# compile-time source generator
/// for regular expressions (<see cref="GeneratedRegexAttribute"/>).
/// </remarks>
internal static partial class PasswordHasherRegex
{
    /// <summary>
    /// Matches valid policy version codes (e.g. "v2", "v3", "v4").
    /// Uses compile-time source generation for optimal performance and zero runtime compilation overhead.
    /// </summary>
    [GeneratedRegex(@"^v[1-9][0-9]*$")]
    public static partial Regex PolicyVersionRegex();
}
