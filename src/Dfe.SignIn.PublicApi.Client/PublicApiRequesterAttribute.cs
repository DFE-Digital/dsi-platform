namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// An attribute that associates an <see cref="Base.Framework.IInteractor{TRequest}"/>
/// implementation with the corresponding public API requester.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class PublicApiRequesterAttribute : Attribute
{
}
