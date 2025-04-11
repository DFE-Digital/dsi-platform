using System.Security.Claims;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Dfe.SignIn.PublicApi.Client.Owin;

/// <summary>
/// Wrapper for the OWIN implementation of <see cref="IHttpContext" />.
/// </summary>
public sealed class HttpContextOwinAdapter(IOwinContext inner)
    : IHttpContext, IHttpRequest, IHttpResponse
{
    #region IHttpContext

    /// <inheritdoc />
    public object Inner => inner;

    /// <inheritdoc />
    public IHttpRequest Request => this;

    /// <inheritdoc />
    public IHttpResponse Response => this;

    /// <inheritdoc />
    public ClaimsPrincipal User => inner.Authentication.User;

    /// <inheritdoc />
    public Task SignInAsync(ClaimsPrincipal newPrincipal)
    {
        var properties = new AuthenticationProperties {
            IsPersistent = true,
        };
        inner.Authentication.SignOut(PublicApiConstants.AuthenticationType);
        inner.Authentication.SignIn(properties, [
            .. newPrincipal.Identities,
        ]);
        return Task.CompletedTask;
    }

    #endregion

    #region IHttpRequest

    /// <inheritdoc />
    string IHttpRequest.Method => inner.Request.Method;

    /// <inheritdoc />
    string IHttpRequest.Scheme => inner.Request.Scheme;

    /// <inheritdoc />
    string IHttpRequest.Host => inner.Request.Host.Value;

    /// <inheritdoc />
    string IHttpRequest.PathBase => inner.Request.PathBase.Value;

    /// <inheritdoc />
    string IHttpRequest.Path => inner.Request.Path.Value;

    /// <inheritdoc />
    async Task<IReadOnlyDictionary<string, StringValues>> IHttpRequest.ReadFormAsync()
    {
        var form = await inner.Request.ReadFormAsync();
        return form.ToDictionary(
            keySelector: entry => entry.Key,
            elementSelector: entry => new StringValues(entry.Value)
        );
    }

    #endregion

    #region IHttpResponse

    /// <inheritdoc />
    void IHttpResponse.Redirect(string location) => inner.Response.Redirect(location);

    #endregion
}
