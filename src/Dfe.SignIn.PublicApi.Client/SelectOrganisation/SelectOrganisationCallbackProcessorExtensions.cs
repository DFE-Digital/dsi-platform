using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Extension methods for <see cref="ISelectOrganisationCallbackProcessor"/>
/// </summary>
public static class SelectOrganisationCallbackProcessorExtensions
{
    /// <summary>
    /// Create view model from "select organisation" callback form.
    /// </summary>
    /// <param name="request">The HTTP request that is handling the callback POST request.</param>
    /// <returns>
    ///   <para>The <see cref="SelectOrganisationCallbackViewModel"/> instance.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    public static SelectOrganisationCallbackViewModel ViewModelFromRequest(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return new SelectOrganisationCallbackViewModel {
            PayloadType = Convert.ToString(request.Form["payloadType"]),
            Payload = Convert.ToString(request.Form["payload"]),
            Sig = Convert.ToString(request.Form["sig"]),
            Kid = Convert.ToString(request.Form["kid"]),
        };
    }

    /// <summary>
    /// Resolves the <see cref="SelectOrganisationCallback"/> type for the given payload type.
    /// </summary>
    /// <param name="payloadType">The type of payload.</param>
    /// <returns>
    ///   <para>The resolved <see cref="SelectOrganisationCallback"/> type.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="payloadType"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="payloadType"/> is an empty string.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="payloadType"/> was unexpected; refer to <see cref="PayloadTypeConstants"/>.</para>
    /// </exception>
    public static Type ResolveCallbackType(string payloadType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadType, nameof(payloadType));

        return payloadType switch {
            PayloadTypeConstants.Error => typeof(SelectOrganisationCallbackError),
            PayloadTypeConstants.Id => typeof(SelectOrganisationCallbackId),
            PayloadTypeConstants.Basic => typeof(SelectOrganisationCallbackBasic),
            PayloadTypeConstants.Extended => typeof(SelectOrganisationCallbackExtended),
            PayloadTypeConstants.Legacy => typeof(SelectOrganisationCallbackLegacy),
            _ => throw new InvalidOperationException($"Unexpected callback type '{payloadType}'."),
        };
    }

    /// <summary>
    /// Verify and process "select organisation" callback data.
    /// </summary>
    /// <param name="processor">The select organisation callback processor instance.</param>
    /// <param name="request">The HTTP request that is handling the callback POST request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The callback data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="processor"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    /// <exception cref="SelectOrganisationCallbackErrorException">
    ///   <para>If a callback error was encountered.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    public static Task<string> ProcessCallbackJsonAsync(
        this ISelectOrganisationCallbackProcessor processor,
        HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(processor, nameof(processor));
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var viewModel = ViewModelFromRequest(request);
        return processor.ProcessCallbackJsonAsync(viewModel, throwOnError: true, cancellationToken);
    }

    /// <summary>
    /// Verify and process "select organisation" callback data.
    /// </summary>
    /// <param name="processor">The select organisation callback processor instance.</param>
    /// <param name="request">The HTTP request that is handling the callback POST request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The callback data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="processor"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    /// <exception cref="SelectOrganisationCallbackErrorException">
    ///   <para>If a callback error was encountered.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    public static async Task<SelectOrganisationCallback> ProcessCallbackAsync(
        this ISelectOrganisationCallbackProcessor processor,
        HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(processor, nameof(processor));
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var viewModel = ViewModelFromRequest(request);
        var targetType = ResolveCallbackType(viewModel.PayloadType);
        return (SelectOrganisationCallback?)await processor.ProcessCallbackRawAsync(
            viewModel, targetType, throwOnError: true, cancellationToken
        ) ?? throw new InvalidOperationException("Invalid callback data.");
    }

    /// <summary>
    /// Verify and process "select organisation" callback data.
    /// </summary>
    /// <typeparam name="TCallback">The type of callback to cast to.</typeparam>
    /// <param name="processor">The select organisation callback processor instance.</param>
    /// <param name="request">The HTTP request that is handling the callback POST request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The callback data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="processor"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    /// <exception cref="SelectOrganisationCallbackErrorException">
    ///   <para>If a callback error was encountered.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    public static async Task<TCallback> ProcessCallbackAsync<TCallback>(
        this ISelectOrganisationCallbackProcessor processor,
        HttpRequest request,
        CancellationToken cancellationToken = default)
        where TCallback : SelectOrganisationCallback
    {
        return (TCallback)await processor.ProcessCallbackAsync(request, cancellationToken);
    }
}
