using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Represents a service that dispatches interaction requests.
/// </summary>
public interface IInteractionDispatcher
{
    /// <summary>
    /// Dispatches an interaction request and awaits a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <param name="request">Request model for the interaction.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The interaction response.</para>
    /// </returns>
    /// <exception cref="InvalidRequestException">
    ///   <para>If the <paramref name="request"/> model is invalid.</para>
    /// </exception>
    /// <exception cref="InvalidResponseException">
    ///   <para>If the response model is invalid.</para>
    /// </exception>
    /// <exception cref="InteractionException">
    ///   <para>If a business domain exception occurs. This should be a custom exception.</para>
    /// </exception>
    /// <exception cref="UnexpectedException">
    ///   <para>If an unexpected exception occurs whilst processing the interaction.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    InteractionTask DispatchAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class;
}

/// <summary>
/// Represents the task of an interaction that has been dispatched.
/// </summary>
/// <remarks>
///   <para>This wrapper is a developer experience to make it simpler to cast the response
///   model to the correct type.</para>
/// </remarks>
/// <param name="task">The actual task.</param>
public struct InteractionTask(Task<object> task)
{
    // Defaults to a task that resolves to `null` to assist mocking in unit tests.
    private readonly Task<object> InnerTask
        => task ?? Task.FromResult<object>(null!);

    /// <summary>
    /// Creates a resolved <see cref="InteractionTask"/> from a given result.
    /// </summary>
    /// <remarks>
    ///   <para>This is equivalent to the <see cref="Task.FromResult{TResult}(TResult)"/> method.</para>
    /// </remarks>
    /// <param name="result">The result.</param>
    /// <returns>
    ///   <para>The resolved interaction task.</para>
    /// </returns>
    public static InteractionTask FromResult(object result)
        => new(Task.FromResult(result));

    /// <summary>
    /// Creates a failed <see cref="InteractionTask"/> from a given exception.
    /// </summary>
    /// <remarks>
    ///   <para>This is equivalent to the <see cref="Task.FromException(Exception)"/> method.</para>
    /// </remarks>
    /// <param name="exception">The exception.</param>
    /// <returns>
    ///   <para>The failed interaction task.</para>
    /// </returns>
    public static InteractionTask FromException(Exception exception)
        => new(Task.FromException<object>(exception));

    /// <summary>
    /// Creates a cancelled <see cref="InteractionTask"/>.
    /// </summary>
    /// <remarks>
    ///   <para>This is equivalent to the <see cref="Task.FromCanceled(CancellationToken)"/> method.</para>
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The cancelled interaction task.</para>
    /// </returns>
    public static InteractionTask FromCanceled(CancellationToken cancellationToken)
        => new(Task.FromCanceled<object>(cancellationToken));

    /// <summary>
    /// Casts a response model to the expected type.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response model.</typeparam>
    /// <returns>
    ///   <para>Response cast as the expected type.</para>
    /// </returns>
    /// <exception cref="InvalidCastException">
    ///   <para>If the specified response model cannot be cast into the expected type.</para>
    /// </exception>
    public readonly async Task<TResponse> To<TResponse>()
        => (TResponse)await this.InnerTask;

    /// <summary>
    /// Gets awaiter for interaction task.
    /// </summary>
    /// <returns>
    ///   <para>Awaiter for interaction task.</para>
    /// </returns>
    public readonly TaskAwaiter<object> GetAwaiter()
        => this.InnerTask.GetAwaiter();
}

/// <summary>
/// A service that dispatches interaction requests using a service provider.
/// </summary>
/// <param name="services">A service provider that resolves concrete interactor implementations.</param>
/// <param name="interactionValidator">A service for validating interaction requests and responses.</param>
public sealed class ServiceProviderInteractionDispatcher(
    IServiceProvider services,
    IInteractionValidator interactionValidator
) : IInteractionDispatcher
{
    /// <inheritdoc/>
    public InteractionTask DispatchAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
    {
        return new InteractionTask(this.DoDispatchAsync(request, cancellationToken));
    }

    private async Task<object> DoDispatchAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
    {
        var interactor = services.GetService<IInteractor<TRequest>>()
            ?? throw new MissingInteractorException(null, typeof(TRequest).Name);

        var context = new InteractionContext<TRequest>(request);

        try {
            interactionValidator.TryValidateRequest(request, context.ValidationResults);

            return await interactor.InvokeAsync(context, cancellationToken);
        }
        catch (InvalidRequestException ex) {
            ThrowUnexpectedException(ex, ex.InvocationId != context.InvocationId);
            throw; // Keep compiler happy.
        }
        catch (InvalidResponseException ex) {
            ThrowUnexpectedException(ex, ex.InvocationId != context.InvocationId);
            throw; // Keep compiler happy.
        }
        catch (Exception ex) {
            // Wrap unexpected internal exceptions.
            ThrowUnexpectedException(ex, ex is not InteractionException and not OperationCanceledException);
            throw; // Keep compiler happy.
        }
    }

    private static void ThrowUnexpectedException(Exception ex, bool isInternal)
    {
        if (isInternal) {
            ex = new UnexpectedException("An unexpected exception occurred whilst processing interaction.", ex);
        }
        ExceptionDispatchInfo.Capture(ex).Throw();
    }
}
