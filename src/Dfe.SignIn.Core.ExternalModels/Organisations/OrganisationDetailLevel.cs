namespace Dfe.SignIn.Core.ExternalModels.Organisations;

/// <summary>
/// Indicates the level of detail to be included in the organisation response.
/// </summary>
public enum OrganisationDetailLevel
{
    /// <summary>
    /// Indicates that only the organisation ID will be included in the callback response.
    /// </summary>
    Id = 0,

    /// <summary>
    /// Indicates that all basic organisation properties (name, category, etc) will be
    /// included in the callback response; in addition to <see cref="Id"/>.
    /// </summary>
    /// <remarks>
    ///   <para>Included properties:</para>
    ///   <list type="bullet">
    ///     <item><c>name</c> - Human friendly name of the organisation.</item>
    ///     <item><c>legalName</c> - Legal name of the organisation.</item>
    ///     <item><c>category</c> - Category of the organisation.</item>
    ///     <item><c>type</c> - Type of organisation.</item>
    ///     <item><c>urn</c> - Unique reference number.</item>
    ///     <item><c>uid</c> - Unique identifier.</item>
    ///     <item><c>upin</c> - Unique provider identification number.</item>
    ///     <item><c>ukprn</c> - UK provider reference number.</item>
    ///     <item><c>establishmentNumber</c> - Code that identifies which establishment.</item>
    ///     <item><c>localAuthority</c> - Local authority if applicable.</item>
    ///     <item><c>status</c> - Status of the organisation.</item>
    ///     <item><c>closedOn</c> - Indicates when the organisation closed.</item>
    ///   </list>
    /// </remarks>
    Basic = 1,

    /// <summary>
    /// Indicates that additional organisation properties (address, telephone, etc) will
    /// be included in the callback response; in addition to <see cref="Basic"/>.
    /// </summary>
    /// <remarks>
    ///   <para>Included properties:</para>
    ///   <list type="bullet">
    ///     <item><c>address</c> - Non-structured address.</item>
    ///     <item><c>telephone</c> - Non-structured phone number.</item>
    ///     <item><c>statutoryLowAge</c> - Lower age of student in organisation if applicable.</item>
    ///     <item><c>statutoryHighAge</c> - Higher age of student in organisation if applicable.</item>
    ///     <item><c>companyRegistrationNumber</c> - As per companies house.</item>
    ///     <item><c>isOnApar</c> - Indicates whether the organisation is on the apprenticeship register.</item>
    ///   </list>
    /// </remarks>
    Extended = 2,

    /// <summary>
    /// Indicates that additional organisation properties (legacyId) will be included
    /// in the callback response; in addition to <see cref="Extended"/>.
    /// </summary>
    /// <remarks>
    ///   <para>Included properties:</para>
    ///   <list type="bullet">
    ///     <item><c>legacyId</c> - A unique ID from an older version of the system.</item>
    ///   </list>
    /// </remarks>
    Legacy = 3,
}
