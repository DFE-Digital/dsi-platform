namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// Constant values representing callback parameter names.
/// </summary>
public static class CallbackParamNames
{
    /// <summary>
    /// Name of the parameter that specifies the unique ID of the "select organisation" request.
    /// </summary>
    /// <remarks>
    ///   <para>Parameter value type: <see cref="Guid"/>.</para>
    /// </remarks>
    public const string RequestId = "rid";

    /// <summary>
    /// Name of the parameter that specifies the type of callback.
    /// </summary>
    /// <remarks>
    ///   <para>Parameter value type: <see cref="string"/> (see <see cref="CallbackTypes"/>).</para>
    /// </remarks>
    public const string Type = "type";

    /// <summary>
    /// Name of the parameter that specifies the error code.
    /// </summary>
    /// <remarks>
    ///   <para>Parameter value type: <see cref="string"/> (see <see cref="SelectOrganisationErrorCode"/>).</para>
    /// </remarks>
    public const string ErrorCode = "code";

    /// <summary>
    /// Name of parameter that specifies the ID of the selected organisation.
    /// </summary>
    /// <remarks>
    ///   <para>Parameter value type: <see cref="Guid"/>.</para>
    /// </remarks>
    public const string Selection = "id";
}
