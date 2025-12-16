using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Public.SelectOrganisation;

namespace Dfe.SignIn.Core.Contracts.SelectOrganisation;

/// <summary>
/// Represents a request to create a new "select organisation" session.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="CreateSelectOrganisationSessionResponse"/></item>
///   </list>
/// </remarks>
public sealed record CreateSelectOrganisationSessionRequest
{
    /// <summary>
    /// The unique DfE Sign-in client ID of the application.
    /// </summary>
    [MinLength(1)]
    public required string ClientId { get; init; }

    /// <summary>
    /// The unique DfE Sign-in ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The prompt that will be presented to the user when they are making their
    /// selection using the "select organisation" web frontend.
    /// </summary>
    public SelectOrganisationPrompt Prompt { get; init; } = new SelectOrganisationPrompt {
        Heading = "Which organisation would you like to use?",
        Hint = "You are associated with more than one organisation. Select one option.",
    };

    /// <summary>
    /// The organisation filtering specification.
    /// </summary>
    public OrganisationFilter Filter { get; init; } = new OrganisationFilter();

    /// <summary>
    /// A value indicating if the user can cancel selection.
    /// </summary>
    public bool AllowCancel { get; init; } = true;

    /// <summary>
    /// The callback URL where the selected organisation response will be posted.
    /// </summary>
    public required Uri CallbackUrl { get; init; }
}

/// <summary>
/// Represents a response for <see cref="CreateSelectOrganisationSessionRequest"/>.
/// </summary>
public sealed record CreateSelectOrganisationSessionResponse
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
    /// A value indicating whether there is at least one option for the user to select from.
    /// </summary>
    /// <remarks>
    ///   <para>An implementor can choose to avoid redirecting to the given <see cref="Url"/>
    ///   when <see cref="HasOptions"/> is false.</para>
    ///   <list type="bullet">
    ///     <item>A value of true indicates that there are one or more options
    ///     for the user to select from.</item>
    ///     <item>A value of false indicates that there are zero options for the
    ///     user to select from.</item>
    ///   </list>
    /// </remarks>
    public required bool HasOptions { get; init; }

    /// <summary>
    /// The generated URL which the user can be redirected to so that they can select an
    /// organisation. Verify if the user has options with <see cref="HasOptions"/>.
    /// </summary>
    public required Uri Url { get; init; }
}
