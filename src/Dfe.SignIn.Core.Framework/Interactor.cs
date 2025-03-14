namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Represents an interaction within the system.
/// </summary>
/// <remarks>
///   <para>Interactors should be added to service collections with the transient lifetime:</para>
///   <code language="csharp"><![CDATA[]]>
///     services.AddTransient<
///         IInteractor<GetExampleByIdRequest, GetExampleByIdResponse>,
///         GetExampleById_ApiRequester
///     >();
///   </code>
/// </remarks>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface IInteractor<TRequest, TResponse>
{
    /// <summary>
    /// Invokes an interaction request.
    /// </summary>
    /// <param name="request">The interaction request.</param>
    /// <returns>
    ///   <para>The interaction response.</para>
    /// </returns>
    /// <seealso cref="IUseCaseHandler{TRequest, TResponse}"/>
    /// <seealso cref="IApiRequester{TRequest, TResponse}"/>
    Task<TResponse> InvokeAsync(TRequest request);
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
///       public sealed class GetExampleById_UseCaseHandler : IGetExampleById
///       {
///           public Task<GetExampleByIdResponse> InvokeAsync(GetExampleByIdRequest request)
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
///       public sealed class GetExampleById_ApiRequester : IGetExampleById
///       {
///           public Task<GetExampleByIdResponse> InvokeAsync(GetExampleByIdRequest request)
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
