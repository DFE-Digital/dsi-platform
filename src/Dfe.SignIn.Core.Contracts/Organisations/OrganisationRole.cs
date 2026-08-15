namespace Dfe.SignIn.Core.Contracts.Organisations;

public sealed partial record OrganisationRole
{
    /// <summary>
    /// The standard end user role (Value = 0, Name = "End user").
    /// </summary>
    public static readonly OrganisationRole EndUser = new( 0, "End user" );

    /// <summary>
    /// The approver role (Value = 10000, Name = "Approver").
    /// </summary>
    public static readonly OrganisationRole Approver = new( 10000, "Approver" );
}

/// <summary>
/// Represents an organisation role with an identifier and a display name.
/// </summary>
public sealed partial record OrganisationRole
{
    /// <summary>
    /// Gets the unique short identifier for the role.
    /// </summary>
    public short Value { get; }

    /// <summary>
    /// Gets the display name of the role.
    /// </summary>
    public string Name { get; }

    private OrganisationRole( short value, string name )
    {
        this.Value = value;
        this.Name = name;
    }

    private static readonly IReadOnlyDictionary<short, OrganisationRole> ById =
        new Dictionary<short, OrganisationRole> {
            [EndUser.Value] = EndUser,
            [Approver.Value] = Approver
        };

    /// <summary>
    /// Gets a known <see cref="OrganisationRole"/> by its Value, or null if not found.
    /// </summary>
    public static OrganisationRole? FromValue( short value )
        => ById.TryGetValue( value, out var role ) ? role : null;
}
