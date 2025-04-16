using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// Assertions for types.
/// </summary>
public static class TypeAssert
{
    /// <summary>
    /// Asserts that a value is an instance of a particular type.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="value">The value that is to be tested.</param>
    /// <returns>
    ///   <para>The type casted value.</para>
    /// </returns>
    /// <exception cref="AssertFailedException">
    ///   <para>If <paramref name="value"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <typeparamref name="T"/> is not in the inheritance hierarchy
    ///   of <paramref name="value"/>.</para>
    /// </exception>
    public static T IsType<T>(object? value)
    {
        Assert.IsInstanceOfType<T>(value);
        return (T)value;
    }

    /// <summary>
    /// Asserts that an action result is a view model of a particular type.
    /// </summary>
    /// <typeparam name="TViewModel">The expected type of view model.</typeparam>
    /// <param name="result">The action result that is to be tested.</param>
    /// <returns>
    ///   <para>The type casted value.</para>
    /// </returns>
    /// <exception cref="AssertFailedException">
    ///   <para>If <paramref name="result"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <see cref="ViewResult"/> is not in the inheritance hierarchy
    ///   of <paramref name="result"/>.</para>
    ///   <para>- or -</para>
    ///   <para>If <typeparamref name="TViewModel"/> is not in the inheritance hierarchy
    ///   of the view model.</para>
    /// </exception>
    public static TViewModel IsViewModelType<TViewModel>(IActionResult result)
    {
        Assert.IsNotNull(result);
        var viewResult = IsType<ViewResult>(result);
        return IsType<TViewModel>(viewResult.Model);
    }
}
