using Dfe.SignIn.Base.Framework;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// AutoMocker extensions to facilitate testing of interactors.
/// </summary>
public static class InteractionAutoMockerExtensions
{
    /// <summary>
    /// Sets up a mock to captures an interaction request model upon being dispatched
    /// using the <see cref="IInteractionDispatcher"/> service.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="captureRequest">A callback to capture the request model.</param>
    /// <param name="response">The fake response.</param>
    public static void CaptureRequest<TRequest>(
        this AutoMocker autoMocker, Action<TRequest> captureRequest, object? response = null)
        where TRequest : class
    {
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<InteractionContext<TRequest>>()
            ))
            .Callback<InteractionContext<TRequest>>(
                context => captureRequest(context.Request)
            )
            .Returns(InteractionTask.FromResult(response!));
    }

    /// <summary>
    /// Sets up a mock to captures an interaction request context upon being dispatched
    /// using the <see cref="IInteractionDispatcher"/> service.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="captureRequest">A callback to capture the request context.</param>
    /// <param name="response">The fake response.</param>
    public static void CaptureInteractionContext<TRequest>(
        this AutoMocker autoMocker, Action<InteractionContext<TRequest>> captureRequest, object? response = null)
        where TRequest : class
    {
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<InteractionContext<TRequest>>()
            ))
            .Callback(captureRequest)
            .Returns(InteractionTask.FromResult(response!));
    }

    /// <summary>
    /// Sets up a mock response where request matches a given predicate.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="response">The fake response.</param>
    public static void MockResponseWhereContext<TRequest>(
        this AutoMocker autoMocker, Predicate<InteractionContext<TRequest>> predicate, object response)
        where TRequest : class
    {
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.Is<InteractionContext<TRequest>>(c => predicate(c))
            ))
            .Returns(InteractionTask.FromResult(response));
    }

    /// <summary>
    /// Sets up a mock response where request matches a given predicate.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="response">The fake response.</param>
    public static void MockResponseWhere<TRequest>(
        this AutoMocker autoMocker, Predicate<TRequest> predicate, object response)
        where TRequest : class
    {
        MockResponseWhereContext<TRequest>(autoMocker, ctx => predicate(ctx.Request), response);
    }

    /// <summary>
    /// Sets up a mock response for a specific request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="response">The fake response.</param>
    public static void MockResponse<TRequest>(this AutoMocker autoMocker, object response)
        where TRequest : class
    {
        MockResponseWhereContext<TRequest>(autoMocker, ctx => true, response);
    }

    /// <summary>
    /// Sets up a mock response matching the given request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="request">The fake request.</param>
    /// <param name="response">The fake response.</param>
    public static void MockResponse<TRequest>(
        this AutoMocker autoMocker, TRequest request, object response)
        where TRequest : class
    {
        MockResponseWhereContext<TRequest>(autoMocker, ctx => Equals(ctx.Request, request), response);
    }

    /// <summary>
    /// Sets up a mock response for a specific request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <typeparam name="TResponse">The type of response model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    public static void MockResponse<TRequest, TResponse>(this AutoMocker autoMocker)
        where TRequest : class
    {
        var response = Activator.CreateInstance<TResponse>()!;
        MockResponseWhereContext<TRequest>(autoMocker, ctx => true, response);
    }

    /// <summary>
    /// Sets up a mock response for the exact request instance.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="request">The fake request.</param>
    /// <param name="response">The fake response.</param>
    public static void MockResponseExactly<TRequest>(
        this AutoMocker autoMocker, TRequest request, object response)
        where TRequest : class
    {
        MockResponseWhereContext<TRequest>(autoMocker, ctx => ReferenceEquals(ctx.Request, request), response);
    }

    /// <summary>
    /// Mock scenario where a request that matches a given predicate throws an exception.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="exception">The fake exception.</param>
    public static void MockThrowsWhereContext<TRequest>(
        this AutoMocker autoMocker, Predicate<InteractionContext<TRequest>> predicate, Exception exception)
        where TRequest : class
    {
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.Is<InteractionContext<TRequest>>(c => predicate(c))
            ))
            .Throws(exception);
    }

    /// <summary>
    /// Mock scenario where a specific request throws an exception.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="exception">The fake exception.</param>
    public static void MockThrowsWhere<TRequest>(this AutoMocker autoMocker, Predicate<TRequest> predicate, Exception exception)
        where TRequest : class
    {
        MockThrowsWhereContext<TRequest>(autoMocker, ctx => predicate(ctx.Request), exception);
    }

    /// <summary>
    /// Mock scenario where a specific request type throws an exception.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="exception">The fake exception.</param>
    public static void MockThrows<TRequest>(this AutoMocker autoMocker, Exception exception)
        where TRequest : class
    {
        MockThrowsWhereContext<TRequest>(autoMocker, _ => true, exception);
    }

    /// <summary>
    /// Mock scenario where a specific request throws an exception.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="request">The fake request.</param>
    /// <param name="exception">The fake exception.</param>
    public static void MockThrows<TRequest>(this AutoMocker autoMocker, TRequest request, Exception exception)
        where TRequest : class
    {
        MockThrowsWhereContext<TRequest>(autoMocker, ctx => Equals(request, ctx.Request), exception);
    }

    /// <summary>
    /// Mock scenario where an exact request throws an exception.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="request">The fake request.</param>
    /// <param name="exception">The fake exception.</param>
    public static void MockThrowsExactly<TRequest>(this AutoMocker autoMocker, TRequest request, Exception exception)
        where TRequest : class
    {
        MockThrowsWhereContext<TRequest>(autoMocker, ctx => ReferenceEquals(request, ctx.Request), exception);
    }

    /// <summary>
    /// Mock scenario where a validation error occurs for a specific request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="requestPropertyName">Name of the invalid request property.</param>
    public static void MockValidationError<TRequest>(this AutoMocker autoMocker, string requestPropertyName)
        where TRequest : class
    {
        MockThrows<TRequest>(autoMocker, new InvalidRequestException(Guid.Empty, [
            new("Invalid value.", [requestPropertyName])
        ]));
    }

    /// <summary>
    /// Mock scenario where a validation error occurs for a specific request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="request">The fake request.</param>
    /// <param name="requestPropertyName">Name of the invalid request property.</param>
    public static void MockValidationError<TRequest>(this AutoMocker autoMocker, TRequest request, string requestPropertyName)
        where TRequest : class
    {
        MockThrows(autoMocker, request, new InvalidRequestException(Guid.Empty, [
            new("Invalid value.", [requestPropertyName])
        ]));
    }

    /// <summary>
    /// Mock scenario where a validation error occurs for an exact request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="request">The fake request.</param>
    /// <param name="requestPropertyName">Name of the invalid request property.</param>
    public static void MockValidationErrorExactly<TRequest>(this AutoMocker autoMocker, TRequest request, string requestPropertyName)
        where TRequest : class
    {
        MockThrowsExactly(autoMocker, request, new InvalidRequestException(Guid.Empty, [
            new("Invalid value.", [requestPropertyName])
        ]));
    }

    /// <summary>
    /// Mock scenario where the <see cref="IInteractionLimiter"/> has rejected the request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="reject">Indicates if the request was rejected by the limited.</param>
    public static void MockLimiter<TRequest>(this AutoMocker autoMocker, bool reject)
        where TRequest : class, IKeyedRequest
    {
        autoMocker.GetMock<IInteractionLimiter>()
            .Setup(x => x.LimitActionAsync(
                It.Is<IKeyedRequest>(req => req is TRequest)
            ))
            .ReturnsAsync(new InteractionLimiterResult {
                WasRejected = reject,
            });
    }

    /// <summary>
    /// Mock scenario where the <see cref="IInteractionLimiter"/> has rejected the request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="request">The fake request.</param>
    /// <param name="reject">Indicates if the request was rejected by the limited.</param>
    public static void MockLimiter<TRequest>(this AutoMocker autoMocker, TRequest request, bool reject)
        where TRequest : class, IKeyedRequest
    {
        autoMocker.GetMock<IInteractionLimiter>()
            .Setup(x => x.LimitActionAsync(
                It.Is<IKeyedRequest>(req => ReferenceEquals(req, request))
            ))
            .ReturnsAsync(new InteractionLimiterResult {
                WasRejected = reject,
            });
    }
}
