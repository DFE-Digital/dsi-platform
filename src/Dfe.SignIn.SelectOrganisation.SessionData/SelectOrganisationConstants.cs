using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.SelectOrganisation.SessionData;

/// <summary>
/// Defines library constants.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SelectOrganisationConstants
{
    /// <summary>
    /// A unique value that identifies the <see cref="IDistributedCache"/> that is to
    /// be used with dependency injection.
    /// </summary>
    public const string CacheStoreKey = "ee86309b-8f3a-4eac-943c-dc14a3c60343";
}
