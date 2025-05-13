using System.Security.Claims;
using Microsoft.Extensions.Primitives;

namespace Dfe.SignIn.PublicApi.Client.Abstractions;

/// <summary>
/// An abstraction representing some sort of HTTP context implementation.
/// </summary>
public interface IHttpContext
{
    /// <summary>
    /// Gets the inner HTTP context implementation.
    /// </summary>
    object Inner { get; }

    /// <summary>
    /// Gets the current request.
    /// </summary>
    IHttpRequest Request { get; }

    /// <summary>
    /// Gets the current response.
    /// </summary>
    IHttpResponse Response { get; }

    /// <summary>
    /// Gets the current user.
    /// </summary>
    ClaimsPrincipal User { get; }

    /// <summary>
    /// Signs in with the new <see cref="ClaimsPrincipal"/> instance.
    /// </summary>
    /// <param name="newPrincipal">New claims principal representing the user.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="newPrincipal"/> is null.</para>
    /// </exception>
    Task SignInAsync(ClaimsPrincipal newPrincipal);
}

/// <summary>
/// An abstraction representing some sort of HTTP request implementation.
/// </summary>
public interface IHttpRequest
{
    /// <summary>
    /// Gets the request method (eg. "POST").
    /// </summary>
    string Method { get; }

    /// <summary>
    /// Gets the scheme of the request (eg. "https").
    /// </summary>
    string Scheme { get; }

    /// <summary>
    /// Gets the host name of the request (eg. "localhost:3000").
    /// </summary>
    string Host { get; }

    /// <summary>
    /// Gets the base path of the request (eg. "/app").
    /// </summary>
    string PathBase { get; }

    /// <summary>
    /// Gets the path of the request (eg. "/callback").
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Reads form values.
    /// </summary>
    /// <returns>
    ///   <para>A read-only dictionary of form values where each key can have
    ///   one or more values.</para>
    /// </returns>
    Task<IReadOnlyDictionary<string, StringValues>> ReadFormAsync();

    /// <summary>
    /// Gets a named value from the query string.
    /// </summary>
    /// <param name="key">Name of the value.</param>
    /// <returns>
    ///   <para>The value when present; otherwise, a value of <c>null</c>.</para>
    /// </returns>
    string? GetQuery(string key);
}

/// <summary>
/// An abstraction representing some sort of HTTP response implementation.
/// </summary>
public interface IHttpResponse
{
    /// <summary>
    /// Returns a temporary redirect response (302) to the client.
    /// </summary>
    /// <param name="location">The URL to redirect the client to. This must be
    /// properly encoded for use in http headers where only ASCII characters
    /// are allowed.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="location" /> is null.</para>
    /// </exception>
    void Redirect(string location);
}
