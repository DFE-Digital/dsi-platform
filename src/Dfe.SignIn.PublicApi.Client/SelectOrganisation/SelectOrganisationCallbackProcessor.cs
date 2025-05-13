using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Represents a service that verifies and processes "select organisation" callback data.
/// </summary>
public interface ISelectOrganisationCallbackProcessor
{
    /// <summary>
    /// Verify and process "select organisation" callback data.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="viewModel">A view model representing the callback form data.</param>
    /// <param name="throwOnError">Indicates if an exception should be thrown for callback errors.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The deserialized callback data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="viewModel"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If payload data is null.</para>
    /// </exception>
    /// <exception cref="SelectOrganisationCallbackErrorException">
    ///   <para>If <paramref name="throwOnError"/> is true and a callback error was encountered.</para>
    /// </exception>
    /// <exception cref="MismatchedCallbackException">
    ///   <para>If callback user ID does not match the user ID from the primary identity.</para>
    ///   <para>- or -</para>
    ///   <para>If callback is provided with an unexpected request ID.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<SelectOrganisationCallback> ProcessCallbackAsync(
        IHttpContext context,
        SelectOrganisationCallbackViewModel viewModel,
        bool throwOnError = true,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// The default implementation for <see cref="ISelectOrganisationCallbackProcessor"/>.
/// </summary>
/// <param name="trackingProvider">Service for tracking the active request to select an organisation.</param>
/// <param name="payloadVerifier">Service for verifying callback payload data.</param>
/// <param name="jsonOptionsAccessor">Provides access to JSON serializer options.</param>
public sealed class SelectOrganisationCallbackProcessor(
    ISelectOrganisationRequestTrackingProvider trackingProvider,
    IPayloadVerifier payloadVerifier,
    IOptionsMonitor<JsonSerializerOptions> jsonOptionsAccessor
) : ISelectOrganisationCallbackProcessor
{
    /// <inheritdoc/>
    public async Task<SelectOrganisationCallback> ProcessCallbackAsync(
        IHttpContext context,
        SelectOrganisationCallbackViewModel viewModel,
        bool throwOnError = true,
        CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNull(viewModel, nameof(viewModel));

        // Verify that the callback was the one that was expected.
        Guid requestId = Guid.Parse(context.Request.GetQuery("rid"));
        if (!await trackingProvider.IsTrackingRequestAsync(context, requestId)) {
            throw new MismatchedCallbackException();
        }
        // Stop tracking to prevent retry attacks.
        await trackingProvider.SetTrackedRequestAsync(context, null);

        var signature = new PayloadDigitalSignature {
            KeyId = viewModel.Kid,
            Signature = viewModel.Sig,
        };
        if (!await payloadVerifier.VerifyPayload(viewModel.Payload, signature)) {
            throw new InvalidOperationException("Invalid payload.");
        }

        Type targetType = SelectOrganisationCallback.ResolveType(viewModel.PayloadType);
        byte[] payloadJson = Convert.FromBase64String(viewModel.Payload);

        var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);

        if (JsonSerializer.Deserialize(payloadJson, targetType, jsonOptions) is not SelectOrganisationCallback data) {
            throw new InvalidOperationException("Callback data was null.");
        }

        Guid currentUserId = context.User.GetDsiUserId();
        if (data.UserId != currentUserId) {
            throw new MismatchedCallbackException();
        }

        if (data is SelectOrganisationCallbackError errorData && throwOnError) {
            throw new SelectOrganisationCallbackErrorException(errorData.Code);
        }

        return data;
    }
}
