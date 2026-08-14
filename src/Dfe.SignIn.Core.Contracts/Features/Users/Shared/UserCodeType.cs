namespace Dfe.SignIn.Core.Contracts.Features.Users.Shared;

/// <summary>
/// Represents the type of user code used for various user actions, such as changing email or resetting password.
/// </summary>
public enum UserCodeType
{
    /// <summary>
    /// Represents a user code type used for changing a user's email address.
    /// </summary>
    ChangeEmail,

    /// <summary>
    /// Represents a user code type used for password reset actions.
    /// </summary>
    PasswordReset,
}

/// <summary>
/// Provides extension methods for the <see cref="UserCodeType"/> enum to convert it to database values.
/// </summary>
public static class UserCodeTypeExtensions
{
    /// <summary>
    /// Converts the <see cref="UserCodeType"/> enum value to its corresponding database string representation.
    /// </summary>
    /// <param name="codeType">The <see cref="UserCodeType"/> enum value to convert.</param>
    /// <returns>The database string representation of the <see cref="UserCodeType"/> enum value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="UserCodeType"/> value is not recognized.</exception>
    public static string ToDbValue(this UserCodeType codeType)
    {
        return codeType switch {
            UserCodeType.ChangeEmail => "changeemail",
            UserCodeType.PasswordReset => "PasswordReset",
            _ => throw new ArgumentOutOfRangeException(nameof(codeType), codeType, null)
        };
    }
}
