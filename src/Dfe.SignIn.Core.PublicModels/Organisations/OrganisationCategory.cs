using System.ComponentModel;

namespace Dfe.SignIn.Core.PublicModels.Organisations;

/// <summary>
/// Specifies the category of an organisation.
/// </summary>
public enum OrganisationCategory
{
    /// <summary>
    /// An establishment.
    /// </summary>
    /// <seealso cref="EstablishmentType"/>
    [Description("Establishment")]
    Establishment = 1,

    /// <summary>
    /// A local authority.
    /// </summary>
    [Description("Local Authority")]
    LocalAuthority = 2,

    /// <summary>
    /// Other legacy organisations.
    /// </summary>
    [Description("Other Legacy Organisations")]
    OtherLegacyOrganisations = 3,

    /// <summary>
    /// Other stakeholders.
    /// </summary>
    [Description("Other Stakeholders")]
    OtherStakeholders = 8,

    /// <summary>
    /// Training providers.
    /// </summary>
    [Description("Training Providers")]
    TrainingProviders = 9,

    /// <summary>
    /// Multi-academy trust.
    /// </summary>
    [Description("Multi-Academy Trust")]
    MultiAcademyTrust = 10,

    /// <summary>
    /// Government.
    /// </summary>
    [Description("Government")]
    Government = 11,

    /// <summary>
    /// Other "Get Information About Schools" (GIAS) stakeholder.
    /// </summary>
    [Description("Other GIAS Stakeholder")]
    OtherGiasStakeholder = 12,

    /// <summary>
    /// Single-academy trust.
    /// </summary>
    [Description("Single-Academy Trust")]
    SingleAcademyTrust = 13,

    /// <summary>
    /// Secure single-academy trust (Secure SAT).
    /// </summary>
    [Description("Secure Single-Academy Trust")]
    SecureSingleAcademyTrust = 14,

    /// <summary>
    /// Software suppliers.
    /// </summary>
    [Description("Software Suppliers")]
    SoftwareSuppliers = 50,

    /// <summary>
    /// PIMS training providers.
    /// </summary>
    [Description("PIMS Training Providers")]
    PimsTrainingProviders = 51,

    /// <summary>
    /// Billing authority.
    /// </summary>
    [Description("Billing Authority")]
    BillingAuthority = 52,

    /// <summary>
    /// Youth custody service.
    /// </summary>
    [Description("Youth Custody Service")]
    YouthCustodyService = 53,
}
