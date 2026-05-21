namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Represents an organisation role with an identifier and a display name.
/// </summary>
/// <param name="Id">The unique short identifier for the role.</param>
/// <param name="Name">The display name of the role.</param>
public sealed record OrganisationRole(short Id, string Name);

/// <summary>
/// Provides static references and lookup methods for well-known organisation roles.
/// </summary>
public static class OrganisationRoles
{
    /// <summary>
    /// The standard end user role (Id = 0, Name = "End user").
    /// </summary>
    public static readonly OrganisationRole EndUser = new(0, "End user");

    /// <summary>
    /// The approver role (Id = 10000, Name = "Approver").
    /// </summary>
    public static readonly OrganisationRole Approver = new(10000, "Approver");

    /// <summary>
    /// Internal lookup dictionary for roles by their short Id.
    /// </summary>
    private static readonly IReadOnlyDictionary<short, OrganisationRole> ById =
        new Dictionary<short, OrganisationRole> {
            [EndUser.Id] = EndUser,
            [Approver.Id] = Approver
        };

    /// <summary>
    /// Gets a known <see cref="OrganisationRole"/> by its Id, or null if not found.
    /// </summary>
    /// <param name="roleId">The short Id of the role.</param>
    /// <returns>The matching <see cref="OrganisationRole"/>, or null if not found.</returns>
    public static OrganisationRole? FromId(short roleId) => ById.TryGetValue(roleId, out var role) ? role : null;
}
