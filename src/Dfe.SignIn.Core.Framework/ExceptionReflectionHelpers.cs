using System.Reflection;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Internal helper reflection functionality to discover serializable properties
/// in interaction exceptions.
/// </summary>
internal static class ExceptionReflectionHelpers
{
    private static IReadOnlyDictionary<string, Type>? exceptionTypesByFullName;
    private static readonly Dictionary<Type, IEnumerable<PropertyInfo>> propertiesByType = [];
    private static readonly object @lock = new();

    /// <summary>
    /// Gets a cached read-only lookup map of exception types by full name.
    /// </summary>
    /// <remarks>
    ///   <para>This function is slower the first time that it is invoked.</para>
    /// </remarks>
    private static IReadOnlyDictionary<string, Type> ExceptionTypesByFullName {
        get {
            lock (@lock) {
                if (exceptionTypesByFullName is not null) {
                    return exceptionTypesByFullName;
                }

                exceptionTypesByFullName = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsAssignableTo(typeof(Exception)))
                    .DistinctBy(type => type.FullName)
                    .ToDictionary(
                        keySelector: type => type.FullName!,
                        elementSelector: type => type
                    );
                return exceptionTypesByFullName;
            }
        }
    }

    /// <summary>
    /// Gets an exception type from a given full name.
    /// </summary>
    /// <param name="fullName">The full name of the exception type.</param>
    /// <returns>
    ///   <para>The exception type when found; otherwise, a value of null.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="property"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="property"/> is empty string.</para>
    /// </exception>
    public static Type? GetExceptionTypeByFullName(string fullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fullName, nameof(fullName));

        ExceptionTypesByFullName.TryGetValue(fullName, out var result);
        return result;
    }

    /// <summary>
    /// Determines whether an exception property can be serialized.
    /// </summary>
    /// <remarks>
    ///   <para>A property can only be serialized where:</para>
    ///   <list type="bullet">
    ///     <item>The property is defined in a custom <see cref="InteractionException"/> type.</item>
    ///     <item>The property is type is not in the <see cref="System.Reflection"/> namespace.</item>
    ///     <item>The property is type is not <see cref="Type"/>.</item>
    ///   </list>
    /// </remarks>
    /// <param name="property">The property.</param>
    /// <returns>
    ///   <para>A value of true if the property can be serialized; otherwise, false.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="property"/> is null.</para>
    /// </exception>
    public static bool IsSerializableProperty(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property, nameof(property));

        // Get property from declaring type so that private accessors are found.
        property = property.DeclaringType?.GetProperty(
            property.Name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        )!;

        return property is not null
            && property.PropertyType != typeof(Type)
            && property.PropertyType.Namespace != typeof(MemberInfo).Namespace
            && property.GetCustomAttribute<PersistAttribute>() is not null
            && property.GetGetMethod(nonPublic: true) is not null
            && property.GetSetMethod(nonPublic: true) is not null;
    }

    /// <summary>
    /// Get serializable properties of the given exception type.
    /// </summary>
    /// <remarks>
    ///   <para>Type information is cached to avoid poor performance due to excessive
    ///   use of reflection.</para>
    ///   <para>This function is thread-safe.</para>
    /// </remarks>
    /// <param name="exceptionType">The type of exception.</param>
    /// <returns>
    ///   <para>An enumerable collection of zero or more properties.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="exceptionType"/> is null.</para>
    /// </exception>
    public static IEnumerable<PropertyInfo> GetSerializableExceptionProperties(Type exceptionType)
    {
        ArgumentNullException.ThrowIfNull(exceptionType, nameof(exceptionType));

        IEnumerable<PropertyInfo> result;
        lock (@lock) {
            if (!propertiesByType.TryGetValue(exceptionType, out result!)) {
                result = [.. exceptionType.GetProperties().Where(IsSerializableProperty)];
                propertiesByType[exceptionType] = result;
            }
        }
        return result;
    }
}
