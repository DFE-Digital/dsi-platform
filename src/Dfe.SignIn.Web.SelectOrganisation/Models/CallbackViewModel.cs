using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.Web.SelectOrganisation.Models;

/// <summary>
/// View model for the "Select organisation" service callback.
/// </summary>
public sealed class CallbackViewModel
{
    /// <summary>
    /// Gets the callback URL.
    /// </summary>
    public required Uri CallbackUrl { get; init; }

    /// <summary>
    /// Gets the type of payload data being included.
    /// </summary>
    /// <remarks>
    ///   <para>This will be one of the following values:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <see cref="PayloadTypeConstants.Error"/> - Indicates that an error has occurred
    ///       (see <see cref="SelectOrganisationCallbackError"/>).
    ///     </item>
    ///     <item>
    ///       <see cref="PayloadTypeConstants.SignOut"/> - Indicates that the user wants to
    ///       sign out (see <see cref="SelectOrganisationCallbackSignOut"/>).
    ///     </item>
    ///     <item>
    ///       <see cref="PayloadTypeConstants.Cancel"/> - Indicates that the user wants to
    ///       cancel selection (see <see cref="SelectOrganisationCallbackCancel"/>).
    ///     </item>
    ///     <item>
    ///       <see cref="PayloadTypeConstants.Selection"/> - Indicates that an organisation
    ///       selection was made (see <see cref="SelectOrganisationCallbackSelection"/>).
    ///     </item>
    ///   </list>
    /// </remarks>
    public required string PayloadType { get; init; }

    /// <summary>
    /// Gets the JSON encoded payload data.
    /// </summary>
    public required string PayloadData { get; init; }

    /// <summary>
    /// Gets the digital signature of the payload data.
    /// </summary>
    public required string DigitalSignature { get; init; }

    /// <summary>
    /// Gets the key of the public key that can be used to verify the digital
    /// signature of the payload data.
    /// </summary>
    public required string PublicKeyId { get; init; }
}
