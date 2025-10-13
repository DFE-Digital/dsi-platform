using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Interfaces.Jobs;

/// <summary>
/// Represents a service that dispatches job requests.
/// </summary>
public interface IJobDispatcher
{
    /// <summary>
    /// Dispatches a job request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <param name="request">The job request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidRequestException">
    ///   <para>If the request model is invalid.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task DispatchAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class;
}
