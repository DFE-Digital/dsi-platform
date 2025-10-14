namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Represents an interaction within the system.
/// </summary>
/// <remarks>
///   <para>Interactors should be added to service collections with the transient lifetime:</para>
///   <code language="csharp"><![CDATA[
///     services.AddInteractor<GetExampleById_ApiRequester>();
///   ]]></code>
///   <para>Or manually with:</para>
///   <code language="csharp"><![CDATA[
///     services.AddTransient<IInteractor<GetExampleByIdRequest>, GetExampleById_ApiRequester>();
///   ]]></code>
///   <para>A request type can implement the <see cref="Caching.ICacheableRequest"/> interface to
///   allow for the caching of interaction responses.</para>
/// </remarks>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <seealso cref="Interactor{TRequest, TResponse}"/>
public interface IInteractor<TRequest>
    where TRequest : class
{
    /// <summary>
    /// Invokes an interaction request.
    /// </summary>
    /// <param name="context">Context of the interaction.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The interaction response.</para>
    /// </returns>
    /// <exception cref="InvalidRequestException">
    ///   <para>If the request model is invalid.</para>
    /// </exception>
    /// <exception cref="InteractionException">
    ///   <para>If a business domain exception occurs. This should be a custom exception.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task<object> InvokeAsync(InteractionContext<TRequest> context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an interaction within the system.
/// </summary>
/// <remarks>
///   <para>Interactors should be added to service collections with the transient lifetime:</para>
///   <code language="csharp"><![CDATA[
///     services.AddInteractor<GetExampleById_ApiRequester>();
///   ]]></code>
///   <para>Or manually with:</para>
///   <code language="csharp"><![CDATA[
///     services.AddTransient<IInteractor<GetExampleByIdRequest>, GetExampleById_ApiRequester>();
///   ]]></code>
/// </remarks>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public abstract class Interactor<TRequest, TResponse> : IInteractor<TRequest>
    where TRequest : class
    where TResponse : class
{
    /// <inheritdoc cref="IInteractor{TRequest}.InvokeAsync(InteractionContext{TRequest}, CancellationToken)"/>
    public abstract Task<TResponse> InvokeAsync(InteractionContext<TRequest> context, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    async Task<object> IInteractor<TRequest>.InvokeAsync(InteractionContext<TRequest> context, CancellationToken cancellationToken)
    {
        return await this.InvokeAsync(context, cancellationToken);
    }
}

/// <summary>
/// Marks an API requester within the system.
/// </summary>
/// <remarks>
///   <para>Whilst an API requester cannot directly handle a request; it can forward
///   a request to a service that can handle it.</para>
///   <example>
///     <para>An example implementation:</para>
///     <code language="csharp"><![CDATA[
///       [ApiRequester]
///       public sealed class GetExampleById_ApiRequester
///           : Interactor<GetExampleByIdRequest, GetExampleByIdResponse>
///       {
///           public override Task<GetExampleByIdResponse> InvokeAsync(
///               InteractionContext<GetExampleByIdRequest> context,
///               CancellationToken cancellationToken = default)
///           {
///               context.ThrowIfHasValidationErrors();
///
///               return Task.FromResult(
///                   new GetExampleByIdResponse {
///                       Name = "Example response value",
///                   }
///               );
///           }
///       }
///     ]]></code>
///   </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ApiRequesterAttribute : Attribute
{
}
