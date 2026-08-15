namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Represents a role within an organisation, defined as a smart enum with a unique short identifier and a display name.
/// </summary>
public sealed partial record OrganisationRole : SmartEnum<OrganisationRole, short>
{
    private OrganisationRole( short value, string name ) : base( value, name ) { }

    /// <summary>
    /// The standard end user role (Value = 0, Name = "End user").
    /// </summary>
    public static readonly OrganisationRole EndUser = new( 0, "End user" );

    /// <summary>
    /// The approver role (Value = 10000, Name = "Approver").
    /// </summary>
    public static readonly OrganisationRole Approver = new( 10000, "Approver" );
}
