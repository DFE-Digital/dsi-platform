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
/// Marks an interactor contract within the system.
/// </summary>
/// <remarks>
///   <example>
///     <para>An example implementation:</para>
///     <code language="csharp"><![CDATA[
///       [InteractorContract]
///       public interface IGetExampleByIdUseCase
///           : IInteractor<GetExampleByIdRequest, GetExampleByIdResponse>;
///     ]]></code>
///   </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class InteractorContractAttribute : Attribute
{
}

/// <summary>
/// Marks a use case handler within the system.
/// </summary>
/// <remarks>
///   <para>A use case handler has the business logic to interact with models.
///   Whilst a use case handler is unaware of implementation details, such as
///   databases, it is able to interace with the system via gateways.</para>
///   <example>
///     <para>An example implementation:</para>
///     <code language="csharp"><![CDATA[
///       [UseCaseHandler]
///       public sealed class GetExampleByIdUseCase
///           : IInteractor<GetExampleByIdRequest, GetExampleByIdResponse>
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
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public sealed class UseCaseHandlerAttribute : Attribute
{
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
///       public sealed class GetExampleByIdUseCase
///           : IInteractor<GetExampleByIdRequest, GetExampleByIdResponse>
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
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ApiRequesterAttribute : Attribute
{
}
