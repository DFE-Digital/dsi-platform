using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// View model for "select organisation" callback data.
/// </summary>
public sealed record SelectOrganisationCallbackViewModel()
{
    /// <summary>
    /// Create view model from "select organisation" callback form.
    /// </summary>
    /// <param name="request">The HTTP request that is handling the callback POST request.</param>
    /// <returns>
    ///   <para>The <see cref="SelectOrganisationCallbackViewModel"/> instance.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    public static async Task<SelectOrganisationCallbackViewModel> FromRequest(IHttpRequest request)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        var form = await request.ReadFormAsync();

        return new SelectOrganisationCallbackViewModel {
            PayloadType = Convert.ToString(form["payloadType"]),
            Payload = Convert.ToString(form["payload"]),
            Sig = Convert.ToString(form["sig"]),
            Kid = Convert.ToString(form["kid"]),
        };
    }

    /// <summary>
    /// Gets the type of payload (eg. "error", "id", "basic", "extended", "legacy").
    /// </summary>
    public required string PayloadType { get; init; }

    /// <summary>
    /// Gets the raw base-64 encoded payload data in JSON format.
    /// </summary>
    /// <remarks>
    ///   <para>This data can be deserialized into the appropriate callback model:</para>
    ///   <list type="bullet">
    ///     <item><see cref="SelectOrganisationCallbackError"/></item>
    ///     <item><see cref="SelectOrganisationCallbackId"/></item>
    ///     <item><see cref="SelectOrganisationCallbackBasic"/></item>
    ///     <item><see cref="SelectOrganisationCallbackExtended"/></item>
    ///     <item><see cref="SelectOrganisationCallbackLegacy"/></item>
    ///   </list>
    /// </remarks>
    public required string Payload { get; init; }

    /// <summary>
    /// Gets the digital signature of the payload data.
    /// </summary>
    public required string Sig { get; init; }

    /// <summary>
    /// Gets the unique ID of the public key that can be used to verify the digital signature.
    /// </summary>
    public required string Kid { get; init; }
}
