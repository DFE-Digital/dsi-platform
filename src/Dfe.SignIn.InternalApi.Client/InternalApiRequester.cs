using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts;
using Dfe.SignIn.InternalApi.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// An interactor that initiates requests to the internal API.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
public sealed class InternalApiRequester<TRequest>(
    [FromKeyedServices(ServiceCollectionExtensions.InternalApiKey)] HttpClient client,
    IOptionsMonitor<JsonSerializerOptions> jsonOptionsAccessor,
    IExceptionJsonSerializer exceptionSerializer
) : IInteractor<TRequest>
    where TRequest : class
{
    private static readonly Assembly ContractsAssembly = typeof(CoreContractsAssembly).Assembly;

    /// <inheritdoc/>
    public async Task<object> InvokeAsync(
        InteractionContext<TRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        string endpointPath = NamingHelpers.GetEndpointPath<TRequest>();

        var responseMessage = await client.PostAsJsonAsync(endpointPath, context.Request, cancellationToken);
        responseMessage.EnsureSuccessStatusCode();

        var response = (await responseMessage.Content.ReadFromJsonAsync<InteractionResponse>(cancellationToken))!;
        if (response.Success) {
            var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
            var responseType = ContractsAssembly.GetType(response.Content.Type, throwOnError: true)!;
            return JsonSerializer.Deserialize(response.Content.Data, responseType, jsonOptions)!;
        }
        else {
            var exception = exceptionSerializer.DeserializeExceptionFromJson(response.Exception.Value);
            throw exception;
        }
    }
}
