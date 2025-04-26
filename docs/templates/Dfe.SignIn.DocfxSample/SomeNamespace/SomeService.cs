namespace Dfe.SignIn.DocfxSample.SomeNamespace;

/// <summary>
/// An example class.
/// </summary>
public interface ISomeService
{
    /// <summary>
    /// Invoke some service.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="anotherInput">Another input string.</param>
    /// <returns>
    ///   <para>Output string.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="input"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="anotherInput"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="input"/> is an empty string.</para>
    /// </exception>
    string Invoke(string input, string anotherInput);
}

/// <summary>
/// A concrete implementation of <see cref="ISomeService"/>.
/// </summary>
/// <remarks>
///   <para>This is an actual implementation.</para>
/// </remarks>
public sealed class SomeService : ISomeService
{
    /// <inheritdoc />
    public string Invoke(string input, string anotherInput)
    {
        ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));
        ArgumentException.ThrowIfNullOrEmpty(anotherInput, nameof(anotherInput));

        return input + anotherInput;
    }
}
