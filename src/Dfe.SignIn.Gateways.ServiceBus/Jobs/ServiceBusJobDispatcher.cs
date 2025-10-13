using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.Jobs;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.ServiceBus.Jobs;

/// <summary>
/// A service that serializes job requests and pushes them to a Service Bus queue.
/// </summary>
public sealed class ServiceBusJobDispatcher(
    IInteractionValidator interactionValidator,
    IOptionsMonitor<JsonSerializerOptions> jsonOptionsAccessor,
    ServiceBusSender sender) : IJobDispatcher
{
    /// <inheritdoc/>
    public Task DispatchAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        var validationResults = new List<ValidationResult>();
        if (!interactionValidator.TryValidateRequest(request, validationResults)) {
            throw new InvalidRequestException(Guid.Empty, validationResults);
        }

        var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
        string requestJson = JsonSerializer.Serialize(request, jsonOptions);

        var message = new ServiceBusMessage(requestJson) {
            Subject = request.GetType().FullName,
        };

        return sender.SendMessageAsync(message, cancellationToken);
    }
}
