using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Represents a request to create a URL where the end-user can select an organisation.
/// </summary>
public sealed record CreateSelectOrganisationSession_PublicApiRequest()
{
    /// <summary>
    /// Callback URL where the selected organisation response will be posted.
    /// </summary>
    [ExampleValue("https://example-service.localhost")]
    public required Uri CallbackUrl { get; init; }

    /// <summary>
    /// Unique DfE Sign-in ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Prompt that will be presented to the user when they are making their selection
    /// using the "select organisation" web frontend.
    /// </summary>
    public SelectOrganisationPrompt Prompt { get; init; } = new SelectOrganisationPrompt {
        Heading = "Which organisation would you like to use?",
        Hint = "You are associated with more than one organisation. Select one option.",
    };

    /// <summary>
    /// Level of organisation detail required in the callback response.
    /// </summary>
    public OrganisationDetailLevel DetailLevel { get; init; } = OrganisationDetailLevel.Basic;

    /// <summary>
    /// Organisation filtering specification.
    /// </summary>
    public OrganisationFilter Filter { get; init; } = new OrganisationFilter();

    /// <summary>
    /// A value indicating if the user can cancel selection.
    /// </summary>
    public bool AllowCancel { get; init; } = true;
}

/// <summary>
/// Response from creating a URL where the end-user can select an organisation..
/// </summary>
public sealed record CreateSelectOrganisationSession_PublicApiResponse()
{
    /// <summary>
    /// A value indicating whether there is at least one option for the user
    /// to select from.
    /// </summary>
    /// <remarks>
    ///   <para>An implementor can choose to avoid redirecting to the given <see cref="Url"/>
    ///   when <see cref="HasOptions"/> is false.</para>
    ///   <list type="bullet">
    ///     <item>A value of <c>true</c> indicates that there are one or more options
    ///     for the user to select from.</item>
    ///     <item>A value of <c>false</c> indicates that there are zero options for the
    ///     user to select from.</item>
    ///   </list>
    /// </remarks>
    public required bool HasOptions { get; init; }

    /// <summary>
    /// URL where the user can be redirected to select an organisation.
    /// </summary>
    public required Uri Url { get; init; }
}
