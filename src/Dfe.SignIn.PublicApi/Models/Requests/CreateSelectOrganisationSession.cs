using Dfe.SignIn.Core.PublicModels.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Models.Requests;

/// <summary>
/// Represents a request to create a new "select organisation" session.
/// </summary>
public sealed record CreateSelectOrganisationSessionApiRequest()
{
    /// <summary>
    /// Gets the callback URL where the selected organisation response will be posted.
    /// </summary>
    public required Uri CallbackUrl { get; init; }

    /// <summary>
    /// Gets the unique DfE Sign-in ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the prompt that will be presented to the user when they are making
    /// their selection using the "select organisation" web frontend.
    /// </summary>
    public SelectOrganisationPrompt Prompt { get; init; } = new SelectOrganisationPrompt {
        Heading = "Which organisation would you like to use?",
        Hint = "You are associated with more than one organisation. Select one option.",
    };

    /// <summary>
    /// Gets the level of organisation detail required in the callback response.
    /// </summary>
    public OrganisationDetailLevel DetailLevel { get; init; } = OrganisationDetailLevel.Basic;

    /// <summary>
    /// Gets the organisation filtering specification.
    /// </summary>
    public OrganisationFilter Filter { get; init; } = new OrganisationFilter();
}
