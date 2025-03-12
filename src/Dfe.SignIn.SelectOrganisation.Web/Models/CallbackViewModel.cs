using Dfe.SignIn.Core.PublicModels.SelectOrganisation;

namespace Dfe.SignIn.SelectOrganisation.Web.Models;

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
    ///       <c>"error"</c> - Indicates that an error has occurred
    ///       (see <see cref="SelectOrganisationCallbackError"/>).
    ///     </item>
    ///     <item>
    ///       <c>"id"</c> - Payload has unique identifier of the selected organisation
    ///       (see <see cref="SelectOrganisationCallbackId"/>).
    ///     </item>
    ///     <item>
    ///       <c>"basic"</c> - Payload includes basic details for the selected organisation
    ///       (see <see cref="SelectOrganisationCallbackBasic"/>).
    ///     </item>
    ///     <item>
    ///       <c>"extended"</c> - Payload includes extended details for the selected organisation
    ///       (see <see cref="SelectOrganisationCallbackExtended"/>).
    ///     </item>
    ///     <item>
    ///       <c>"legacy"</c> - Payload includes additional legacy properties
    ///       (see <see cref="SelectOrganisationCallbackLegacy"/>).
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
