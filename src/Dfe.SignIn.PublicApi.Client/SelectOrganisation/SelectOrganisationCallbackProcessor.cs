using System.Text;
using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Represents a service that verifies and processes "select organisation" callback data.
/// </summary>
public interface ISelectOrganisationCallbackProcessor
{
    /// <summary>
    /// Verify and process "select organisation" callback data.
    /// </summary>
    /// <param name="viewModel">A view model representing the callback form data.</param>
    /// <param name="throwOnError">Indicates if an exception should be thrown for callback errors.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The raw callback data in JSON format.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="viewModel"/> is null.</para>
    /// </exception>
    /// <exception cref="SelectOrganisationCallbackErrorException">
    ///   <para>If <paramref name="throwOnError"/> is true and a callback error was encountered.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<string> ProcessCallbackJsonAsync(
        SelectOrganisationCallbackViewModel viewModel,
        bool throwOnError,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Verify and process "select organisation" callback data.
    /// </summary>
    /// <param name="viewModel">A view model representing the callback form data.</param>
    /// <param name="targetType">The type of object to deserialize.</param>
    /// <param name="throwOnError">Indicates if an exception should be thrown for callback errors.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The deserialized callback data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="viewModel"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="targetType"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If payload data is null.</para>
    /// </exception>
    /// <exception cref="SelectOrganisationCallbackErrorException">
    ///   <para>If <paramref name="throwOnError"/> is true and a callback error was encountered.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<object> ProcessCallbackRawAsync(
        SelectOrganisationCallbackViewModel viewModel,
        Type targetType,
        bool throwOnError,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// The default implementation for <see cref="ISelectOrganisationCallbackProcessor"/>.
/// </summary>
public sealed class SelectOrganisationCallbackProcessor(
    IPayloadVerifier payloadVerifier,
    [FromKeyedServices(JsonHelperExtensions.StandardOptionsKey)] JsonSerializerOptions jsonOptions
) : ISelectOrganisationCallbackProcessor
{
    /// <inheritdoc/>
    public async Task<string> ProcessCallbackJsonAsync(
        SelectOrganisationCallbackViewModel viewModel,
        bool throwOnError,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewModel, nameof(viewModel));

        var signature = new PayloadDigitalSignature {
            KeyId = viewModel.Kid,
            Signature = viewModel.Sig,
        };
        if (!await payloadVerifier.VerifyPayload(viewModel.Payload, signature)) {
            throw new InvalidOperationException("Invalid payload.");
        }

        var payloadJson = Convert.FromBase64String(viewModel.Payload);

        if (viewModel.PayloadType == PayloadTypeConstants.Error && throwOnError) {
            var errorData = JsonSerializer.Deserialize<SelectOrganisationCallbackError>(payloadJson, jsonOptions)!;
            throw new SelectOrganisationCallbackErrorException(errorData.Code);
        }

        return Encoding.UTF8.GetString(payloadJson);
    }

    /// <inheritdoc/>
    public async Task<object> ProcessCallbackRawAsync(
        SelectOrganisationCallbackViewModel viewModel,
        Type targetType,
        bool throwOnError,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(targetType, nameof(targetType));

        var json = await this.ProcessCallbackJsonAsync(viewModel, throwOnError, cancellationToken);

        if (viewModel.PayloadType == PayloadTypeConstants.Error && !throwOnError) {
            targetType = typeof(SelectOrganisationCallbackError);
        }

        return JsonSerializer.Deserialize(json, targetType, jsonOptions)
            ?? throw new InvalidOperationException("Callback data was null.");
    }
}
