using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Extension functionality functionality to assist with DfE Sign-in claims.
/// </summary>
/// <seealso cref="DsiClaimTypes"/>
public static class DsiClaimExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    };

    /// <summary>
    /// Gets the primary identity from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <returns>
    ///   <para>The claims identity representing the primary user identity when found;
    ///   otherwise, a value of null.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    internal static ClaimsIdentity? GetPrimaryIdentity(this ClaimsPrincipal user)
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        return user.Identities.FirstOrDefault(identity =>
            identity.HasClaim(claim => claim.Type == DsiClaimTypes.UserId)
        );
    }

    /// <summary>
    /// Gets the DfE Sign-in organisation identity from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <returns>
    ///   <para>The claims identity representing the selected organisation when found;
    ///   otherwise, a value of null.</para>
    ///   <para>Returns null when session ID does not match that of the primary identity.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    public static ClaimsIdentity? GetDsiOrganisationIdentity(this ClaimsPrincipal user)
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        TryGetSessionId(user, out string? sessionId);
        if (sessionId is null) {
            return null;
        }

        return user.Identities.FirstOrDefault(identity =>
            identity.AuthenticationType == PublicApiConstants.AuthenticationType &&
            identity.HasClaim(DsiClaimTypes.SessionId, sessionId)
        );
    }

    /// <summary>
    /// Gets a unique value that identifies the primary session from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <returns>
    ///   <para>The unique identifier of the primary session.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    /// <exception cref="MissingClaimException">
    ///   <para>If <paramref name="user"/> does not include the "sid" claim; or the
    ///   claim value consists of just whitespace.</para>
    /// </exception>
    internal static string GetSessionId(this ClaimsPrincipal user)
    {
        TryGetSessionId(user, out var sessionId);
        return sessionId ?? throw new MissingClaimException("Missing primary identity with claim.", DsiClaimTypes.SessionId);
    }

    /// <summary>
    /// Tries to get the unique value that identifies the primary session from a
    /// claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <param name="sessionId">Outputs the session ID when present; otherwise, null.</param>
    /// <returns>
    ///   <para>A value of <c>true</c> if the "sid" claim exists; otherwise, a
    ///   value of <c>false</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    /// <exception cref="MissingClaimException">
    ///   <para>If <paramref name="user"/> does not include the "sid" claim; or the
    ///   claim value consists of just whitespace.</para>
    /// </exception>
    internal static bool TryGetSessionId(this ClaimsPrincipal user, out string? sessionId)
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        var primaryIdentity = user.GetPrimaryIdentity();
        sessionId = primaryIdentity?.FindFirst(DsiClaimTypes.SessionId)?.Value;

        return sessionId is not null;
    }

    /// <summary>
    /// Gets the DfE Sign-in user identifier from from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <returns>
    ///   <para>The unique identifier of the user in DfE Sign-in.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    /// <exception cref="MissingClaimException">
    ///   <para>If <paramref name="user"/> does not include the "dsi_user_id" claim.</para>
    /// </exception>
    public static Guid GetDsiUserId(this ClaimsPrincipal user)
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        TryGetDsiUserId(user, out var userId);
        return userId ?? throw new MissingClaimException("Missing identity with claim.", DsiClaimTypes.UserId);
    }

    /// <summary>
    /// Tries to get the DfE Sign-in user identifier from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <param name="userId">Outputs the user ID when present; otherwise, null.</param>
    /// <returns>
    ///   <para>A value of <c>true</c> if the "dsi_user_id" claim exists; otherwise, a
    ///   value of <c>false</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    public static bool TryGetDsiUserId(this ClaimsPrincipal user, out Guid? userId)
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        var claim = user?.FindFirst(DsiClaimTypes.UserId);
        userId = claim is not null
            ? new Guid(claim.Value)
            : null;
        return userId is not null;
    }

    /// <summary>
    /// Gets the DfE Sign-in organisation from the claims principal.
    /// </summary>
    /// <remarks>
    ///   <para>This method is only applicable when using the <see cref="ActiveOrganisationClaimsProvider"/>
    ///   implementation of <see cref="IActiveOrganisationProvider"/>.</para>
    /// </remarks>
    /// <param name="user">Claims principal representing the user.</param>
    /// <returns>
    ///   <para>The organisation that the user is associated with; otherwise,
    ///   a value of <c>null</c>.</para>
    ///   <para>Returns null when session ID does not match that of the primary identity.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    public static TOrganisationDetails? GetDsiOrganisation<TOrganisationDetails>(this ClaimsPrincipal user)
        where TOrganisationDetails : OrganisationDetails
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        var dsiIdentity = user.GetDsiOrganisationIdentity();
        return dsiIdentity?.DeserializeDsiOrganisation<TOrganisationDetails>();
    }

    internal static string SerializeDsiOrganisation(OrganisationDetails? organisation)
    {
        return organisation is not null
            ? JsonSerializer.Serialize(organisation, organisation.GetType(), JsonSerializerOptions)
            : "null";
    }

    internal static TOrganisationDetails? DeserializeDsiOrganisation<TOrganisationDetails>(this ClaimsIdentity user)
        where TOrganisationDetails : OrganisationDetails
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        var claim = user?.FindFirst(DsiClaimTypes.Organisation);
        return claim is not null
            ? JsonSerializer.Deserialize<TOrganisationDetails>(claim.Value, JsonSerializerOptions)
            : null;
    }
}
