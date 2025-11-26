using System.Collections.Concurrent;
using System.Reflection;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.WebFramework.Mvc;

/// <summary>
/// Specifies that a view model property maps to a corresponding property in a request model.
/// </summary>
/// <typeparam name="TRequest">The type of the request model being targeted.</typeparam>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class MapToAttribute<TRequest> : Attribute
    where TRequest : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapToAttribute{TRequest}"/> class.
    /// </summary>
    /// <param name="requestPropertyName">The name of the property in the request model to map to.</param>
    /// <param name="flags">Optional flags that control how the mapping behaves.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="requestPropertyName"/> is null or an empty string.</para>
    /// </exception>
    public MapToAttribute(string requestPropertyName, RequestMappingOptions flags = RequestMappingOptions.Everything)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(requestPropertyName, nameof(requestPropertyName));

        this.RequestPropertyName = requestPropertyName;
        this.Flags = flags;
    }

    /// <summary>
    /// Gets the type of request.
    /// </summary>
    public Type RequestType => typeof(TRequest);

    /// <summary>
    /// Gets the name of the property in the request model that this view model property maps to.
    /// </summary>
    public string RequestPropertyName { get; }

    /// <summary>
    /// Gets the binding flags that define how this mapping should be interpreted.
    /// </summary>
    public RequestMappingOptions Flags { get; }
}

/// <summary>
/// Flags that control how a view model property is mapped to a request model property.
/// </summary>
[Flags]
public enum RequestMappingOptions
{
    /// <summary>
    /// No mapping behavior is specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enables all mapping behaviors (value transfer and validation error mapping).
    /// </summary>
    Everything = ~0,

    /// <summary>
    /// Indicates that the property should be used to transfer values from the view model
    /// to the request.
    /// </summary>
    Value = 0x01,

    /// <summary>
    /// Indicates that validation errors for the request property should be mapped back to
    /// the view model property.
    /// </summary>
    ValidationErrors = 0x02,
}

/// <summary>
/// Provides reflection-based utilities for mapping view model properties to request model
/// properties using <see cref="MapToAttribute{TRequest}"/> annotations.
/// </summary>
internal static class RequestMappingHelpers
{
    /// <summary>
    /// Caches previously computed mappings between view model and request model types
    /// to improve performance and avoid repeated reflection.
    /// </summary>
    private static readonly ConcurrentDictionary<(Type viewModelType, Type requestType), Dictionary<string, RequestPropertyMapping>> Cache = new();

    private const string NonExistentPropertyName = "_";

    /// <summary>
    /// Retrieves a dictionary of property mappings between a view model type and a request
    /// model type, based on <see cref="MapToAttribute{TRequest}"/> annotations.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request model.</typeparam>
    /// <param name="viewModelType">The type of the view model.</param>
    /// <returns>
    ///   <para>A read-only dictionary mapping request property names to <see cref="RequestPropertyMapping"/>
    ///   instances that describe how each view model property maps to the request.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="viewModelType"/> is null or an empty string.</para>
    /// </exception>
    public static IReadOnlyDictionary<string, RequestPropertyMapping> GetMappings<TRequest>(Type viewModelType)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(viewModelType, nameof(viewModelType));

        var key = (viewModelType, typeof(TRequest));
        if (!Cache.TryGetValue(key, out var mappings)) {
            mappings = viewModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(prop => {
                    var annotation = prop.GetCustomAttribute<MapToAttribute<TRequest>>();
                    return (
                        annotation: annotation!,
                        viewModelProperty: prop,
                        requestProperty: typeof(TRequest).GetProperty(
                            // Fallback to non-existent property so that requestProperty becomes `null`.
                            annotation?.RequestPropertyName ?? NonExistentPropertyName,
                            BindingFlags.Public | BindingFlags.Instance
                        )
                    );
                })
                .Where(x => x.requestProperty is not null)
                .Select(x => new RequestPropertyMapping(x.viewModelProperty, x.requestProperty!, x.annotation.Flags))
                .ToDictionary(x => x.RequestProperty.Name);
            Cache[key] = mappings;
        }
        return mappings;
    }
}

/// <summary>
/// Represents a mapping between a view model property and a request model property,
/// along with associated mapping flags.
/// </summary>
/// <param name="viewModelProperty">The source property from the view model.</param>
/// <param name="requestProperty">The target property in the request model.</param>
/// <param name="flags">Flags that define how the mapping should behave.</param>
internal struct RequestPropertyMapping(PropertyInfo viewModelProperty, PropertyInfo requestProperty, RequestMappingOptions flags)
{
    /// <summary>
    /// Gets the source property from the view model.
    /// </summary>
    public readonly PropertyInfo ViewModelProperty => viewModelProperty;

    /// <summary>
    /// Gets the target property in the request model.
    /// </summary>
    public readonly PropertyInfo RequestProperty => requestProperty;

    /// <summary>
    /// Gets the flags that define how this mapping should behave.
    /// </summary>
    public readonly RequestMappingOptions Flags = flags;
}
