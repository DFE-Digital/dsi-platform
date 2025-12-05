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
    public static async Task<object> InteractionHandler<TRequest, TResponse>(
        HttpContext context,
        [FromBody] TRequest request,
        // ---
        ILogger<LoggerContext> logger,
        IExceptionJsonSerializer exceptionSerializer,
        IInteractionDispatcher interaction)
        where TRequest : class
        where TResponse : class
    {
        try {
            var interactionTask = interaction.DispatchAsync(request);
            var response = await interactionTask.To<TResponse>();
            return new InteractionResponse<TResponse> {
                Type = response.GetType().FullName!,
                Data = response,
            };
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (InvalidRequestException ex) {
            logger.LogError(ex, "Invalid request.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return FailedInteractionResponse.FromException(ex, exceptionSerializer);
        }
        catch (Exception ex) {
            logger.LogError(ex, "An error occurred whilst processing internal API request.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return FailedInteractionResponse.FromException(ex, exceptionSerializer);
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
        app.MapPost(path, InteractionHandler<TRequest, TResponse>)
            .RequireAuthorization()
            .Produces<InteractionResponse<TResponse>>(StatusCodes.Status200OK)
            .Produces<FailedInteractionResponse>(StatusCodes.Status400BadRequest)
            .Produces<FailedInteractionResponse>(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
