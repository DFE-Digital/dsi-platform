using System.Reflection;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Helper functionality for discovering interactor types within an assembly.
/// </summary>
public static class InteractorReflectionHelpers
{
    /// <summary>
    /// Discovers all interactor types in an assembly.
    /// </summary>
    /// <remarks>
    ///   <example>
    ///     <para>Discover all interactor types that implement <see cref="IInteractor{TRequest}"/>:</para>
    ///     <code language="csharp"><![CDATA[
    ///       var types = InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(
    ///         typeof(SomeClass).Assembly
    ///       );
    ///     ]]></code>
    ///   </example>
    /// </remarks>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>
    ///   <para>An enumerable collection of interactor type descriptors.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="assembly"/> is null</para>
    /// </exception>
    public static IEnumerable<InteractorTypeDescriptor> DiscoverInteractorTypesInAssembly(Assembly assembly)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly, nameof(assembly));

        return assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .SelectMany(type => type.GetInterfaces(), (type, interfaceType) => new InteractorTypeDescriptor {
                ContractType = interfaceType,
                ConcreteType = type,
            })
            .Where(descriptor =>
                descriptor.ContractType.IsGenericType &&
                descriptor.ContractType.GetGenericTypeDefinition() == typeof(IInteractor<>)
            );
    }

    /// <summary>
    /// Discovers annotated interactors.
    /// </summary>
    /// <typeparam name="TAttribute">The annotation attribute type.</typeparam>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>
    ///   <para>An enumerable collection of interactor type descriptors.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="assembly"/> is null.</para>
    /// </exception>
    public static IEnumerable<InteractorTypeDescriptor> DiscoverAnnotatedInteractorsInAssembly<TAttribute>(Assembly assembly)
        where TAttribute : Attribute
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly, nameof(assembly));

        return assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.GetCustomAttribute<TAttribute>() is not null)
            .Select(type => new InteractorTypeDescriptor {
                ContractType = type.GetInterfaces().FirstOrDefault(interfaceType =>
                    interfaceType.GetGenericTypeDefinition() == typeof(IInteractor<>)
                )!,
                ConcreteType = type,
            })
            .Where(descriptor => descriptor.ContractType is not null);
    }

    /// <summary>
    /// Discovers all use case handler types in an assembly (interactors that are annotated
    /// with the <see cref="ApiRequesterAttribute"/>).
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>
    ///   <para>An enumerable collection of interactor type descriptors.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="assembly"/> is null.</para>
    /// </exception>
    public static IEnumerable<InteractorTypeDescriptor> DiscoverApiRequesterTypesInAssembly(Assembly assembly)
    {
        return DiscoverAnnotatedInteractorsInAssembly<ApiRequesterAttribute>(assembly);
    }
}

/// <summary>
/// Maps an interactor contract type to a concrete implementation type.
/// </summary>
public sealed record InteractorTypeDescriptor
{
    /// <summary>
    /// Gets the interactor contract type; for example,
    /// <c>IInteractor&lt;ExampleRequest, ExampleResponse&gt;</c>.
    /// </summary>
    public required Type ContractType { get; init; }

    /// <summary>
    /// Gets the interactor concrete implementation type; for example,
    /// <c>GetExampleUseCase</c> or <c>GetExampleApiRequester</c>.
    /// </summary>
    public required Type ConcreteType { get; init; }
}
