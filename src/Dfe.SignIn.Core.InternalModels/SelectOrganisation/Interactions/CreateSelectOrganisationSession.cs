using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;

/// <summary>
/// Represents a request to create a new "select organisation" session.
/// </summary>
public sealed record CreateSelectOrganisationSessionRequest()
{
    /// <summary>
    /// Gets the unique DfE Sign-in client ID of the application.
    /// </summary>
    [MinLength(1)]
    public required string ClientId { get; init; }

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
    /// Gets the organisation filtering specification.
    /// </summary>
    public OrganisationFilter Filter { get; init; } = new OrganisationFilter();

    /// <summary>
    /// Gets a value indicating if the user can cancel selection.
    /// </summary>
    public bool AllowCancel { get; init; } = true;

    /// <summary>
    /// Gets the callback URL where the selected organisation response will be posted.
    /// </summary>
    public required Uri CallbackUrl { get; init; }

    /// <summary>
    /// Gets the level of organisation detail required in the callback response.
    /// </summary>
    [EnumDataType(typeof(OrganisationDetailLevel))]
    public OrganisationDetailLevel DetailLevel { get; init; } = OrganisationDetailLevel.Basic;
}

/// <summary>
/// Represents a response for <see cref="CreateSelectOrganisationSessionRequest"/>.
/// </summary>
public sealed record CreateSelectOrganisationSessionResponse()
{
    /// <summary>
    /// A unique value representing the request.
    /// </summary>
    /// <remarks>
    ///   <para>This value can be used by the client application to verify that the
    ///   callback matches the initiated request. If the recieved value does not match
    ///   then it is possible that the callback has been triggered unintentionally.</para>
    /// </remarks>
    public required Guid RequestId { get; init; }

    /// <summary>
    /// Gets a value indicating whether there is at least one option for the user
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
    /// Gets the generated URL which the user can be redirected to so that they
    /// can select an organisation.
    /// </summary>
    public required Uri Url { get; init; }
}
