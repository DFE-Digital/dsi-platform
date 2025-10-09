using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.InternalApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.InternalApi.Endpoints;

/// <summary>
/// Extensions for registering interaction endpoints in the internal API.
/// </summary>
public static class InteractionHandlerExtensions
{
    /// <summary>
    /// Represents the context of logging related to <see cref="InteractionHandlerExtensions"/>.
    /// </summary>
    public sealed class LoggerContext { }

    /// <summary>
    /// Posts an interaction request using the interaction framework.
    /// </summary>
    public static async Task<InteractionResponse<TResponse>> InteractionHandler<TRequest, TResponse>(
        [FromBody] TRequest request,
        // ---
        ILogger<LoggerContext> logger,
        IExceptionJsonSerializer exceptionSerializer,
        IInteractionDispatcher interaction,
        // ---
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        try {
            var interactionTask = interaction.DispatchAsync(request, cancellationToken);
            var response = await interactionTask.To<TResponse>();
            return InteractionResponse<TResponse>.FromResponse(response);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception ex) {
            logger.LogError(ex, "An error occurred whilst processing internal API request.");
            return InteractionResponse<TResponse>.FromException(ex, exceptionSerializer);
        }
    }

    /// <summary>
    /// Maps handler for the specified interaction request/response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of interaction request.</typeparam>
    /// <typeparam name="TResponse">The type of interaction response.</typeparam>
    /// <param name="app">The application instance.</param>
    [ExcludeFromCodeCoverage]
    internal static void Map<TRequest, TResponse>(this WebApplication app)
        where TRequest : class
        where TResponse : class
    {
        string path = NamingHelpers.GetEndpointPath<TRequest>();
        app.MapPost(path, InteractionHandler<TRequest, TResponse>);
    }
}
