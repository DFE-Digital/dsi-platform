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
                It.IsAny<InteractionContext<TRequest>>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback<InteractionContext<TRequest>, CancellationToken>(
                (context, _) => captureRequest(context.Request)
            )
            .Returns(InteractionTask.FromResult(response!));
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
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<InteractionContext<TRequest>>(),
                It.IsAny<CancellationToken>()
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
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.Is<InteractionContext<TRequest>>(c => predicate(c.Request)),
                It.IsAny<CancellationToken>()
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
    public static void MockResponseWhereContext<TRequest>(
        this AutoMocker autoMocker, Predicate<InteractionContext<TRequest>> predicate, object response)
        where TRequest : class
    {
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.Is<InteractionContext<TRequest>>(c => predicate(c)),
                It.IsAny<CancellationToken>()
            ))
            .Returns(InteractionTask.FromResult(response));
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
        MockResponseWhere<TRequest>(autoMocker, r => Equals(r, request), response);
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
        MockResponseWhere<TRequest>(autoMocker, r => ReferenceEquals(r, request), response);
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
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<InteractionContext<TRequest>>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(InteractionTask.FromResult(Activator.CreateInstance<TResponse>()!));
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
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<InteractionContext<TRequest>>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws(exception);
    }

    /// <summary>
    /// Mock scenario where a specific request throws an exception.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <param name="autoMocker">The auto mocker instance.</param>
    /// <param name="request"></param>
    /// <param name="exception">The fake exception.</param>
    public static void MockThrows<TRequest>(this AutoMocker autoMocker, TRequest request, Exception exception)
        where TRequest : class
    {
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.Is<InteractionContext<TRequest>>(c => ReferenceEquals(c.Request, request)),
                It.IsAny<CancellationToken>()
            ))
            .Throws(exception);
    }
}
