namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Represents an interaction within the system.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface IInteractor<TRequest, TResponse>
{
    /// <summary>
    /// Handles the interaction.
    /// </summary>
    /// <param name="request">The interaction request.</param>
    /// <returns>
    ///   <para>The interaction response.</para>
    /// </returns>
    /// <seealso cref="IUseCaseHandler{TRequest, TResponse}"/>
    /// <seealso cref="IApiRequester{TRequest, TResponse}"/>
    Task<TResponse> HandleAsync(TRequest request);
}

/// <summary>
/// Represents a use case handler within the system.
/// </summary>
/// <remarks>
///   <para>A use case handler has the business logic to interact with models.
///   Whilst a use case handler is unaware of implementation details, such as
///   databases, it is able to interace with the system via gateways.</para>
///   <example>
///     <para>An example implementation:</para>
///     <code language="csharp"><![CDATA[
///       public sealed class GetExampleByIdUseCase
///           : IUseCaseHandler<GetExampleByIdRequest, GetExampleByIdResponse>
///       {
///           public Task<GetExampleByIdResponse> HandleAsync(GetExampleByIdRequest request)
///           {
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
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface IUseCaseHandler<TRequest, TResponse>
    : IInteractor<TRequest, TResponse>
{
}

/// <summary>
/// Represents a API requester within the system.
/// </summary>
/// <remarks>
///   <para>Whilst an API requester cannot directly handle a request; it can forward
///   a request to a service that can handle it.</para>
///   <example>
///     <para>An example implementation:</para>
///     <code language="csharp"><![CDATA[
///       public sealed class GetExampleByIdUseCase
///           : IApiRequester<GetExampleByIdRequest, GetExampleByIdResponse>
///       {
///           public Task<GetExampleByIdResponse> HandleAsync(GetExampleByIdRequest request)
///           {
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
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface IApiRequester<TRequest, TResponse>
    : IInteractor<TRequest, TResponse>
{
}
