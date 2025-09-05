using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Public.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Contracts.SelectOrganisation;

/// <summary>
/// Represents a request to create a URL where the end-user can select an organisation.
/// </summary>
public record CreateSelectOrganisationSessionApiRequestBody
{
    /// <summary>
    /// Specifies the callback URL where the selected organisation response will be sent.
    /// </summary>
    [ExampleValue("https://example-service.localhost")]
    public required Uri CallbackUrl { get; init; }

    /// <summary>
    /// Specifies the unique DfE Sign-in ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Specifies the prompt that will be presented to the user when they are making their
    /// selection using the "select organisation" web frontend.
    /// </summary>
    public SelectOrganisationPrompt Prompt { get; init; } = new SelectOrganisationPrompt {
        Heading = "Which organisation would you like to use?",
        Hint = "You are associated with more than one organisation. Select one option.",
    };

    /// <summary>
    /// Specifies the organisation filtering requirements.
    /// </summary>
    public OrganisationFilter Filter { get; init; } = new OrganisationFilter();

    /// <summary>
    /// Specifies if the user can cancel selection.
    /// </summary>
    public bool AllowCancel { get; init; } = true;
}

/// <summary>
/// Represents a request to create a URL where the end-user can select an organisation.
/// </summary>
public sealed record CreateSelectOrganisationSessionApiRequest
    : CreateSelectOrganisationSessionApiRequestBody
{
}

/// <summary>
/// Response to <see cref="CreateSelectOrganisationSessionApiRequest"/>.
/// </summary>
public sealed record CreateSelectOrganisationSessionApiResponse
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
