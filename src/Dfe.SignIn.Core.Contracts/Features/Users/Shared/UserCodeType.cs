namespace Dfe.SignIn.Core.Contracts.Features.Users.Shared;

/// <summary>
/// Represents the type of user code, defined as a smart enum with a unique short identifier and a display name.
/// </summary>
public sealed record UserCodeType : SmartEnum<UserCodeType, string>
{
    /// <inheritdoc/>
    private UserCodeType( string value, string name ) : base( value, name ) { }

    /// <summary>
    /// The user code type for changing a user's email address (Value = "changeemail", Name = "Change Email").
    /// </summary>
    public static readonly UserCodeType ChangeEmail = new( "changeemail", "Change Email" );
    /// <summary>
    /// The user code type for password reset actions (Value = "PasswordReset", Name = "Password Reset").
    /// </summary>
    public static readonly UserCodeType PasswordReset = new( "PasswordReset", "Password Reset" );
}
