namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangePassword;

/// <summary>
/// Provides methods to validate password complexity requirements.
/// </summary>
public static class PasswordRequirements
{
    /// <summary>
    /// The minimum length for a valid password.
    /// </summary>
    public const int MinimumLength = 8;

    /// <summary>
    /// The maximum length for a valid password.
    /// </summary>
    public const int MaximumLength = 64;

    /// <summary>
    /// Determines if the provided password meets the complexity requirements.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>true if the password meets the complexity requirements; otherwise, false.</returns>
    public static bool MeetsComplexityRequirement(string password)
    {
        if (string.IsNullOrEmpty(password)) {
            return false;
        }

        int requirementsMet = 0;
        if (password.Any(char.IsLower)) {
            requirementsMet++;
        }

        if (password.Any(char.IsUpper)) {
            requirementsMet++;
        }

        if (password.Any(char.IsDigit)) {
            requirementsMet++;
        }

        if (password.Any(c => !char.IsLetterOrDigit(c))) {
            requirementsMet++;
        }

        return requirementsMet >= 3;
    }
}
