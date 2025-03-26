namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// An attribute that associates an <see cref="Core.Framework.IInteractor{TRequest,TResponse}"/>
/// implementation with the corresponding public API requester.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class PublicApiRequesterAttribute : Attribute
{
}
