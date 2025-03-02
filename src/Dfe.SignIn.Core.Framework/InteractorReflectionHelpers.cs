using System.Reflection;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Helper functionality for discovering interactor types within an assembly.
/// </summary>
public static class InteractorReflectionHelpers
{
    /// <summary>
    /// Discovers all interactor types in an assembly that implement a specific
    /// generic contract type.
    /// </summary>
    /// <remarks>
    ///   <example>
    ///     <para>Discover all interactor types that implement <see cref="IUseCaseHandler{,}"/>:</para>
    ///     <code language="csharp"><![CDATA[
    ///       var types = InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(
    ///         typeof(SomeClass).Assembly,
    ///         typeof(IUseCaseHandler<,>)
    ///       );
    ///     ]]></code>
    ///   </example>
    /// </remarks>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="genericContractType">Generic contract type.</param>
    /// <returns>
    ///   <para>An enumerable collection of interactor type descriptors.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="assembly"/> is null</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="genericContractType"/> is null</para>
    /// </exception>
    public static IEnumerable<InteractorTypeDescriptor> DiscoverInteractorTypesInAssembly(Assembly assembly, Type genericContractType)
    {
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
        ArgumentNullException.ThrowIfNull(genericContractType, nameof(genericContractType));

        return assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .SelectMany(type => type.GetInterfaces(), (type, interfaceType) => new {
                Type = type,
                InterfaceType = interfaceType,
            })
            .Where(x =>
                x.InterfaceType.IsGenericType &&
                x.InterfaceType.GetGenericTypeDefinition() == genericContractType
            )
            .Select(x => new InteractorTypeDescriptor {
                ContractType = x.Type.GetInterfaces()
                    .First(interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(IInteractor<,>)),
                ConcreteType = x.Type,
            });
    }

    /// <summary>
    /// Discovers all interactor types in an assembly (implement <see cref="IInteractor{,}"/>).
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>
    ///   <para>An enumerable collection of interactor type descriptors.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="assembly"/> is null.</para>
    /// </exception>
    public static IEnumerable<InteractorTypeDescriptor> DiscoverInteractorTypesInAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        return DiscoverInteractorTypesInAssembly(assembly, typeof(IInteractor<,>));
    }

    /// <summary>
    /// Discovers all use case handler types in an assembly (implement <see cref="IUseCaseHandler{,}"/>).
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>
    ///   <para>An enumerable collection of interactor type descriptors.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="assembly"/> is null.</para>
    /// </exception>
    public static IEnumerable<InteractorTypeDescriptor> DiscoverUseCaseHandlerTypesInAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        return DiscoverInteractorTypesInAssembly(assembly, typeof(IUseCaseHandler<,>));
    }

    /// <summary>
    /// Discovers all API requester types in an assembly (implement <see cref="IApiRequester{,}"/>).
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
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        return DiscoverInteractorTypesInAssembly(assembly, typeof(IApiRequester<,>));
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
