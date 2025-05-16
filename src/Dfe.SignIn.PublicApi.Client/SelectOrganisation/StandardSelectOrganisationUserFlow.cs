using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.Users;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// The standard implementation of the "select organisation" user flow.
/// </summary>
/// <param name="optionsAccessor">Provides access to the user flow options.</param>
/// <param name="trackingProvider">A provider that tracks an active selection request.</param>
/// <param name="events">Handlers for the various events that occur throughout selection.</param>
/// <param name="createSelectOrganisationSession">A service representing an interaction with
/// the DfE Sign-in API to create "select organisation" sessions.</param>
/// <param name="queryUserOrganisation">A service representing an interaction with the DfE
/// Sign-in API to query an organisation for a specific user.</param>
public sealed class StandardSelectOrganisationUserFlow(
    IOptions<StandardSelectOrganisationUserFlowOptions> optionsAccessor,
    ISelectOrganisationRequestTrackingProvider trackingProvider,
    ISelectOrganisationEvents events,
    IInteractor<CreateSelectOrganisationSession_PublicApiRequest, CreateSelectOrganisationSession_PublicApiResponse> createSelectOrganisationSession,
    IInteractor<QueryUserOrganisation_PublicApiRequest, QueryUserOrganisation_PublicApiResponse> queryUserOrganisation
) : ISelectOrganisationUserFlow
{
    private static void CheckUserIsAuthenticated(IHttpContext context)
    {
        var primaryIdentity = context.User.GetPrimaryIdentity();
        if (primaryIdentity?.IsAuthenticated != true) {
            throw new InvalidOperationException("User is not authenticated.");
        }
    }

    /// <inheritdoc/>
    public async Task InitiateSelectionAsync(IHttpContext context, bool allowCancel, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        CheckUserIsAuthenticated(context);

        var options = optionsAccessor.Value;

        var request = new CreateSelectOrganisationSession_PublicApiRequest {
            UserId = context.User.GetDsiUserId(),
            CallbackUrl = new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{options.CallbackPath}"),
            AllowCancel = allowCancel,
        };
        if (options.PrepareSelectOrganisationRequest is not null) {
            request = options.PrepareSelectOrganisationRequest(request);
        }

        var selectOrganisationResponse = await createSelectOrganisationSession.InvokeAsync(request, cancellationToken);

        if (selectOrganisationResponse.HasOptions) {
            await trackingProvider.SetTrackedRequestAsync(context, selectOrganisationResponse.RequestId);
            await events.OnStartSelection(context, selectOrganisationResponse.Url);
        }
        else {
            await trackingProvider.SetTrackedRequestAsync(context, null);
            await events.OnConfirmSelection(context, null);
        }
    }

    /// <inheritdoc/>
    public async Task ProcessCallbackAsync(IHttpContext context, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        CheckUserIsAuthenticated(context);

        // Verify that the callback was the one that was expected.
        Guid requestId = Guid.Parse(context.Request.GetRequiredQuery(CallbackParamNames.RequestId));
        if (!await trackingProvider.IsTrackingRequestAsync(context, requestId)) {
            throw new MismatchedCallbackException();
        }

        // Stop tracking to prevent retry attacks.
        await trackingProvider.SetTrackedRequestAsync(context, null);

        string callbackType = context.Request.GetRequiredQuery(CallbackParamNames.Type);
        switch (callbackType) {
            case CallbackTypes.Cancel:
                await events.OnCancelSelection(context);
                break;

            case CallbackTypes.Error:
                await events.OnError(context, ParseErrorCode(context));
                break;

            case CallbackTypes.Selection:
                var organisationDetails = await this.FetchOrganisationDetails(context);
                if (organisationDetails is not null) {
                    await events.OnConfirmSelection(context, organisationDetails);
                }
                else {
                    await events.OnError(context, SelectOrganisationErrorCode.InvalidSelection);
                }
                break;

            case CallbackTypes.SignOut:
                await events.OnSignOut(context);
                break;

            default:
                throw new InvalidOperationException($"Unexpected callback type '{callbackType}'.");
        }
    }

    private static SelectOrganisationErrorCode ParseErrorCode(IHttpContext context)
    {
        string errorCodeRaw = context.Request.GetRequiredQuery(CallbackParamNames.ErrorCode)!;
        if (!Enum.TryParse<SelectOrganisationErrorCode>(errorCodeRaw, out var errorCode)) {
            errorCode = SelectOrganisationErrorCode.InternalError;
        }
        return errorCode;
    }

    private static Guid ParseSelectedOrganisationId(IHttpContext context)
    {
        return new Guid(context.Request.GetRequiredQuery(CallbackParamNames.Selection));
    }

    private async Task<OrganisationDetails?> FetchOrganisationDetails(IHttpContext context)
    {
        var options = optionsAccessor.Value;

        Guid userId = context.User.GetDsiUserId();

        var response = await queryUserOrganisation.InvokeAsync(new() {
            OrganisationId = ParseSelectedOrganisationId(context),
            UserId = userId,
            Filter = options.Filter,
        });

        if (response.UserId != userId) {
            return null;
        }

        return response.Organisation;
    }
}
