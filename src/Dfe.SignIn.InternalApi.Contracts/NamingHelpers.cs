using System.Text.RegularExpressions;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.InternalApi.Contracts;

/// <summary>
/// Naming helpers for interactions with the internal API.
/// </summary>
public sealed partial class NamingHelpers
{
    [GeneratedRegex("^[A-Za-z_][A-Za-z_0-9]*", RegexOptions.Singleline)]
    private static partial Regex IdentifierPattern();

    /// <summary>
    /// Creates a nicer ID for schemas that are generated from generic types.
    /// </summary>
    /// <remarks>
    ///   <para>The names of non-generic types are used where possible.</para>
    ///   <para>For generic types; names are joined on underscores to ensure readability.</para>
    /// </remarks>
    /// <param name="type">The type for which a schema is being generated.</param>
    /// <returns>
    ///   <para>The niceified schema ID which conforms to Open API identifier syntax.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="type"/> is null.</para>
    /// </exception>
    public static string NiceifySchemaId(Type type)
    {
        ExceptionHelpers.ThrowIfArgumentNull(type, nameof(type));

        if (type.IsGenericType) {
            var match = IdentifierPattern().Match(type.Name);
            if (match.Success) {
                var genericArgumentNames = type.GetGenericArguments().Select(x => x.Name);
                return $"{match.Value}_{string.Join('_', genericArgumentNames)}";
            }
        }

        return type.Name;
    }

    [GeneratedRegex(@"^interaction/(.+)\.", RegexOptions.Singleline)]
    private static partial Regex GroupPattern();

    /// <summary>
    /// Infers the group name of an endpoint from its path.
    /// </summary>
    /// <param name="path">The path of the endpoint.</param>
    /// <returns>
    ///   <para>The schema group name.</para>
    /// </returns>
    public static string GetSchemaGroupFromPath(string? path)
    {
        var match = GroupPattern().Match(path ?? "");
        return match.Success
            ? match.Groups[1].Value
            : "Default";
    }

    [GeneratedRegex(@"^Dfe\.SignIn\.Core\.Contracts\.(.+)Request$", RegexOptions.Singleline)]
    private static partial Regex EndpointPathPattern();

    /// <summary>
    /// Gets the path of an internal API interaction endpoint.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <returns>
    ///   <para>The path of the interaction endpoint.</para>
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///   <para>If endpoint path cannot be resolved for the given request type.</para>
    /// </exception>
    public static string GetEndpointPath<TRequest>()
    {
        if (typeof(TRequest).FullName is not null) {
            var match = EndpointPathPattern().Match(typeof(TRequest).FullName!);
            if (match.Success) {
                return $"interaction/{match.Groups[1].Value}";
            }
        }
        throw new InvalidOperationException("Cannot resolve endpoint path for request type.");
    }
}
