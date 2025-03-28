namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Constant values for the DfE Sign-in public API.
/// </summary>
public static class PublicApiConstants
{
    /// <summary>
    /// Gets the unique key for the DfE Sign-in public API <see cref="HttpClient"/>.
    /// </summary>
    public const string HttpClientKey = "866ee36f-f050-4420-a0c1-79ecfeb1f4a8";

    /// <summary>
    /// Name of the DfE Sign-in authentication type.
    /// </summary>
    /// <remarks>
    ///   <para>This is used for the custom DfE Sign-in organisation user identity.</para>
    /// </remarks>
    public const string AuthenticationType = "DfeSignIn";
}
