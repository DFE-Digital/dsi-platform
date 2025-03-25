using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Extension functionality functionality to assist with DfE Sign-in claims.
/// </summary>
/// <seealso cref="DfeSignInClaimTypes"/>
public static class DfeSignInClaimExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };

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
    /// <exception cref="InvalidOperationException">
    ///   <para>If <paramref name="user"/> does not include the claim.</para>
    /// </exception>
    public static Guid GetDsiUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var claim = user.Claims.First(claim => claim.Type == DfeSignInClaimTypes.UserId);
        return new Guid(claim.Value);
    }

    /// <summary>
    /// Tries to get the DfE Sign-in user identifier from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <param name="userId">Outputs the user ID when present; otherwise, null.</param>
    /// <returns>
    ///   <para>A value of <c>true</c> if the claim exists; otherwise, a value
    ///   of <c>false</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    public static bool TryGetDsiUserId(this ClaimsPrincipal user, out Guid? userId)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var claim = user.Claims.FirstOrDefault(claim => claim.Type == DfeSignInClaimTypes.UserId);
        userId = claim is not null
            ? new Guid(claim.Value)
            : null;
        return userId is not null;
    }

    /// <summary>
    /// Gets the DfE Sign-in organisation from the claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <returns>
    ///   <para>The organisation that the user is currently associated with; otherwise,
    ///   a value of <c>null</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    public static OrganisationClaim? GetDsiOrganisation(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var claim = user.Claims.FirstOrDefault(claim => claim.Type == DfeSignInClaimTypes.Organisation);
        return claim is not null
            ? JsonSerializer.Deserialize<OrganisationClaim>(claim.Value, JsonSerializerOptions)
            : null;
    }
}
