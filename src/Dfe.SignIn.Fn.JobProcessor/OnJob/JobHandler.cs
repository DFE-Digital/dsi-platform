using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Fn.JobProcessor.OnJob;

public sealed class JobHandler(
    ILogger<JobHandler> logger,
    IOptionsMonitor<JsonSerializerOptions> jsonOptionsAccessor,
    IInteractionDispatcher interaction)
{
    private static readonly Assembly ContractsAssembly = typeof(CoreContractsAssembly).Assembly;
    private static readonly MethodInfo DispatchAsyncMethod = typeof(IInteractionDispatcher)
        .GetMethod(nameof(IInteractionDispatcher.DispatchAsync), BindingFlags.Public | BindingFlags.Instance)
        ?? throw new MissingMethodException();

    [Function("OnJob")]
    public async Task Run(
        [ServiceBusTrigger("%JobsQueue__Name%", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);

        string requestTypeName = message.ApplicationProperties.GetValueOrDefault("Type")?.ToString()
            ?? throw new InvalidOperationException("Missing 'Type' property.");

        logger.LogInformation(
            "Processing {Type}. CorrelationId: {CorrelationId}",
            requestTypeName, message.CorrelationId
        );

        var requestType = ContractsAssembly.GetType(requestTypeName, throwOnError: true)!;
        var request = await JsonSerializer.DeserializeAsync(
            message.Body.ToStream(), requestType, jsonOptions, cancellationToken)!;

        var dispatchAsyncMethod = DispatchAsyncMethod.MakeGenericMethod(requestType);
        await (InteractionTask)dispatchAsyncMethod.Invoke(interaction, [request, cancellationToken])!;
    }
}
