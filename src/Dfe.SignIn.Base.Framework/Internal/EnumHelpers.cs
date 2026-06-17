
using System.ComponentModel;
using System.Reflection;

namespace Dfe.SignIn.Base.Framework.Internal;

/// <summary>
/// Provides helper methods for working with Enums.
/// </summary>
/// <exclude />
public static class EnumHelpers
{
    /// <summary>
    /// Maps an input (string or numeric) to a non-nullable enum of type <typeparamref name="TEnum"/>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to map to.</typeparam>
    /// <param name="input">The input value (string or numeric) to map.</param>
    /// <returns>The mapped enum value.</returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="input"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="input"/> does not correspond to a valid enum value.</para>
    /// </exception>
    public static TEnum MapEnum<TEnum>(object? input)
        where TEnum : struct, Enum
    {
        ExceptionHelpers.ThrowIfArgumentNull(input!, typeof(TEnum).Name);

        // Determine the type of input and attempt to map it to the enum:
        // - If input is a string, parse it as the enum name (case-insensitive).
        // - If input is an int map it to the corresponding enum value if defined.
        // - If none of these match, throw an ArgumentException.
        return input switch {
            string s when !string.IsNullOrWhiteSpace(s) => (TEnum)Enum.Parse(typeof(TEnum), s, ignoreCase: true),
            int i when Enum.IsDefined(typeof(TEnum), i) => (TEnum)(object)i,
            _ => throw new ArgumentException($"Cannot convert '{input}' to enum {typeof(TEnum).Name}"),
        };
    }

    /// <summary>
    /// Return the Description decorator from the enumeration.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString());

        if (member.Length > 0) {
            var attribute = member[0]
                .GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null) {
                return attribute.Description;
            }
        }

        // Fallback if no DescriptionAttribute is found
        return value.ToString();
    }
}
