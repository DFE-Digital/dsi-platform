using System.Security.Claims;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore;

/// <summary>
/// Wrapper for the .NET Core implementation of <see cref="IHttpContext" />.
/// </summary>
/// <param name="inner">The inner <see cref="HttpContext"/> instance.</param>
public sealed class HttpContextAspNetCoreAdapter(HttpContext inner)
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
    public ClaimsPrincipal User => inner.User;

    /// <inheritdoc />
    public Task SignInAsync(ClaimsPrincipal newPrincipal) => inner.SignInAsync(newPrincipal);

    #endregion

    #region IHttpRequest

    /// <inheritdoc />
    string IHttpRequest.Method => inner.Request.Method;

    /// <inheritdoc />
    string IHttpRequest.Scheme => inner.Request.Scheme;

    /// <inheritdoc />
    string IHttpRequest.Host => inner.Request.Host.Value;

    /// <inheritdoc />
    string IHttpRequest.PathBase => inner.Request.PathBase;

    /// <inheritdoc />
    string IHttpRequest.Path => inner.Request.Path;

    /// <inheritdoc />
    async Task<IReadOnlyDictionary<string, StringValues>> IHttpRequest.ReadFormAsync()
    {
        var form = await inner.Request.ReadFormAsync();
        return form.ToDictionary(
            keySelector: entry => entry.Key,
            elementSelector: entry => entry.Value
        );
    }

    /// <inheritdoc />
    string? IHttpRequest.GetQuery(string key)
    {
        ExceptionHelpers.ThrowIfArgumentNull(key, nameof(key));
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(key, nameof(key));

        inner.Request.Query.TryGetValue(key, out var values);
        return values.FirstOrDefault();
    }

    #endregion

    #region IHttpResponse

    /// <inheritdoc />
    void IHttpResponse.Redirect(string location) => inner.Response.Redirect(location);

    #endregion
}
