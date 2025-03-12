using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.SessionData;

/// <summary>
/// Options for distributed cache implementation of "select organisation" sessions.
/// </summary>
/// <seealso cref="SelectOrganisationSessionCacheExtensions.AddSelectOrganisationSessionCache"/>
public sealed class SelectOrganisationSessionCacheOptions : IOptions<SelectOrganisationSessionCacheOptions>
{
    /// <summary>
    /// Gets or sets the cache key prefix.
    /// </summary>
    /// <remarks>
    ///   <para>This defaults to a value of "session_".</para>
    /// </remarks>
    public string CacheKeyPrefix { get; set; } = "session:";

    /// <inheritdoc/>
    SelectOrganisationSessionCacheOptions IOptions<SelectOrganisationSessionCacheOptions>.Value => this;
}
