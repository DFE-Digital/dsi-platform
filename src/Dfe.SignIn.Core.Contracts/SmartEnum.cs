using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Dfe.SignIn.Core.Contracts;

/// <summary>
/// Defines a contract for a smart enum that can be parsed from a value and provides a list of all defined instances.
/// </summary>
/// <typeparam name="TEnum">The type of the smart enum.</typeparam>
/// <typeparam name="TValue">The type of the value of the smart enum.</typeparam>
public interface IParsableSmartEnum<
    [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicFields )] TEnum,
    TValue>
    where TEnum : IParsableSmartEnum<TEnum, TValue>
    where TValue : notnull, IEquatable<TValue>
{
    /// <summary>
    /// Returns the smart enum instance of type <typeparamref name="TEnum"/> that corresponds to the specified value.
    /// </summary>
    /// <param name="value">The value of the smart enum instance to retrieve.</param>
    /// <returns>The smart enum instance that corresponds to the specified value, or null if not found.</returns>
    static abstract TEnum? FromValue( TValue value );

    /// <summary>
    /// Gets a read-only collection of all defined smart enum instances of type <typeparamref name="TEnum"/>.
    /// </summary>
    static abstract IReadOnlyCollection<TEnum> List { get; }
}

/// <summary>
/// Represents a smart enum with a unique identifier and a display name.
/// </summary>
/// <typeparam name="TEnum">The type of the smart enum.</typeparam>
/// <typeparam name="TValue">The type of the value of the smart enum.</typeparam>
public abstract record SmartEnum<
    [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicFields )] TEnum,
    TValue> : IParsableSmartEnum<TEnum, TValue>
    where TEnum : SmartEnum<TEnum, TValue>
    where TValue : notnull, IEquatable<TValue>
{
    private static readonly Lazy<Dictionary<TValue, TEnum>> ValueLookup = new( () =>
        typeof( TEnum )
            .GetFields( BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly )
            .Where( f => f.FieldType == typeof( TEnum ) )
            .Select( f => (TEnum)f.GetValue( null )! )
            .ToDictionary( item => item.Value ) );

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartEnum{TEnum, TValue}"/> class with the specified value and name.
    /// </summary>
    /// <param name="value">The unique identifier for the smart enum instance.</param>
    /// <param name="name">The display name of the smart enum instance.</param>
    protected SmartEnum( TValue value, string name )
    {
        this.Value = value;
        this.Name = name;
    }

    /// <summary>
    /// Gets the unique identifier for the smart enum instance.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Gets the display name of the smart enum instance.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Returns the smart enum instance of type <typeparamref name="TEnum"/> that corresponds to the specified value.
    /// </summary>
    /// <param name="value">The value of the smart enum instance to retrieve.</param>
    /// <returns>The smart enum instance that corresponds to the specified value, or null if not found.</returns>
    public static TEnum? FromValue( TValue value )
        => ValueLookup.Value.GetValueOrDefault( value );

    /// <summary>
    /// Gets a read-only collection of all defined smart enum instances of type <typeparamref name="TEnum"/>.
    /// </summary>
    public static IReadOnlyCollection<TEnum> List
        => ValueLookup.Value.Values;

    /// <summary>
    /// Returns the display name of the smart enum instance.
    /// </summary>
    /// <returns>The display name of the smart enum instance.</returns>
    public sealed override string ToString()
        => this.Name;
}
