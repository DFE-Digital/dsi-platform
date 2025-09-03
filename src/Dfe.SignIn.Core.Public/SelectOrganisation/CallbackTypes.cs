namespace Dfe.SignIn.Core.Public.SelectOrganisation;

/// <summary>
/// Constant values representing the different types of callback resulting from a
/// "select organisation" user flow.
/// </summary>
public static class CallbackTypes
{
    /// <summary>
    /// Indicates that an error occurred.
    /// </summary>
    /// <remarks>
    ///   <para>The following parameters are available for callbacks of this type:</para>
    ///   <list type="bullet">
    ///     <item><c>rid</c> - Unique ID representing the select organisation request.</item>
    ///     <item><c>code</c> - Error code; see <see cref="SelectOrganisationErrorCode"/>.</item>
    ///   </list>
    /// </remarks>
    public const string Error = "error";

    /// <summary>
    /// Indicates that the user chose to sign out.
    /// </summary>
    /// <remarks>
    ///   <para>The following parameters are available for callbacks of this type:</para>
    ///   <list type="bullet">
    ///     <item><c>rid</c> - Unique ID representing the select organisation request.</item>
    ///   </list>
    /// </remarks>
    public const string SignOut = "signOut";

    /// <summary>
    /// Indicates that the user chose to cancel the select organisation user flow.
    /// </summary>
    /// <remarks>
    ///   <para>The following parameters are available for callbacks of this type:</para>
    ///   <list type="bullet">
    ///     <item><c>rid</c> - Unique ID representing the select organisation request.</item>
    ///   </list>
    /// </remarks>
    public const string Cancel = "cancel";

    /// <summary>
    /// Indicates that an organisation was selected.
    /// </summary>
    /// <remarks>
    ///   <para>The following parameters are available for callbacks of this type:</para>
    ///   <list type="bullet">
    ///     <item><c>rid</c> - Unique ID representing the select organisation request.</item>
    ///     <item><c>selection</c> - The unique ID representing the organisation.</item>
    ///   </list>
    /// </remarks>
    public const string Selection = "selection";
}
