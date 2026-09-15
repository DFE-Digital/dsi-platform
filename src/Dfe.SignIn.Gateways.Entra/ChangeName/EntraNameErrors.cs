using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.Gateways.Entra.ChangeName;

/// <summary>
/// Strongly-typed errors for Entra name operations.
/// </summary>
public static class EntraNameErrors
{
    /// <summary>
    /// Indicates that the external user ID provided was empty.
    /// </summary>
    public const string InvalidUserIdCode = "Entra.Name.InvalidUserId";

    /// <summary>
    /// Indicates that the first name provided was null, empty, or whitespace.
    /// </summary>
    public const string InvalidFirstNameCode = "Entra.Name.InvalidFirstName";

    /// <summary>
    /// Indicates that the last name provided was null, empty, or whitespace.
    /// </summary>
    public const string InvalidLastNameCode = "Entra.Name.InvalidLastName";

    /// <summary>
    /// Indicates that the user with the specified Entra OID was not found.
    /// </summary>
    public const string UserNotFoundCode = "Entra.Name.UserNotFound";

    /// <summary>
    /// Indicates that updating the user's name in Entra failed.
    /// </summary>
    public const string UserUpdateFailedCode = "Entra.Name.UserUpdateFailed";

    /// <summary>
    /// Indicates an unexpected error occurred during Entra name synchronization.
    /// </summary>
    public const string UnexpectedErrorCode = "Entra.Name.Unexpected";

    /// <summary>
    /// Error indicating that the external user ID provided was empty.
    /// </summary>
    public static readonly Error InvalidUserId =
        new(InvalidUserIdCode, "External user ID must not be empty.", "externalUserId");

    /// <summary>
    /// Error indicating that the first name provided was null, empty, or whitespace.
    /// </summary>
    public static readonly Error InvalidFirstName =
        new(InvalidFirstNameCode, "First name must not be empty or whitespace.", "newFirstName");

    /// <summary>
    /// Error indicating that the last name provided was null, empty, or whitespace.
    /// </summary>
    public static readonly Error InvalidLastName =
        new(InvalidLastNameCode, "Last name must not be empty or whitespace.", "newLastName");

    /// <summary>
    /// Creates an error indicating that the user with the specified Entra OID was not found.
    /// </summary>
    /// <param name="userId">The Entra OID of the user.</param>
    /// <returns>The created error.</returns>
    public static Error UserNotFound(Guid userId)
        => new(UserNotFoundCode, $"User with Entra OID '{userId}' was not found.");

    /// <summary>
    /// Creates an error indicating that updating the user's name in Entra failed.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error UserUpdateFailed(string detail)
        => new(UserUpdateFailedCode, $"Failed to update user name in Entra: {detail}");

    /// <summary>
    /// Creates an error indicating an unexpected error occurred during Entra name synchronization.
    /// </summary>
    /// <param name="detail">The detail of the error.</param>
    /// <returns>The created error.</returns>
    public static Error Unexpected(string detail)
        => new(UnexpectedErrorCode, $"An unexpected error occurred during Entra name synchronization: {detail}");
}
