using System.ComponentModel;
using Dfe.SignIn.Core.ExternalModels.Metadata;

namespace Dfe.SignIn.Core.ExternalModels.Organisations;

/// <summary>
/// Specifies the status of an organisation.
/// </summary>
public enum OrganisationStatus
{
    /// <summary>
    /// Hidden.
    /// </summary>
    [Description("Hidden"), TagColour(TagColour.Grey)]
    Hidden = 0,

    /// <summary>
    /// Open.
    /// </summary>
    [Description("Open"), TagColour(TagColour.Green)]
    Open = 1,

    /// <summary>
    /// Closed.
    /// </summary>
    [Description("Closed"), TagColour(TagColour.Red)]
    Closed = 2,

    /// <summary>
    /// Proposed to close.
    /// </summary>
    [Description("Proposed to close"), TagColour(TagColour.Orange)]
    ProposedToClose = 3,

    /// <summary>
    /// Proposed to open.
    /// </summary>
    [Description("Proposed to open"), TagColour(TagColour.Blue)]
    ProposedToOpen = 4,

    /// <summary>
    /// Dissolved.
    /// </summary>
    [Description("Dissolved"), TagColour(TagColour.Red)]
    Dissolved = 5,

    /// <summary>
    /// In liquidation.
    /// </summary>
    [Description("In Liquidation"), TagColour(TagColour.Red)]
    InLiquidation = 6,

    /// <summary>
    /// Locked duplicate.
    /// </summary>
    [Description("Locked Duplicate"), TagColour(TagColour.Purple)]
    LockedDuplicate = 8,

    /// <summary>
    /// Created in error.
    /// </summary>
    [Description("Created in error"), TagColour(TagColour.Red)]
    CreatedInError = 9,

    /// <summary>
    /// Locked restructure.
    /// </summary>
    [Description("Locked Restructure"), TagColour(TagColour.Purple)]
    LockedRestructure = 10,
}
