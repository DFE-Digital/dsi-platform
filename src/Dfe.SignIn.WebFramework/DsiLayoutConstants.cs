namespace Dfe.SignIn.WebFramework;

/// <summary>
/// Constants for the DfE Sign-in "_DsiLayout" shared layout.
/// </summary>
public static class DsiLayoutConstants
{
    /// <summary>
    /// View data key for title text.
    /// </summary>
    /// <remarks>
    ///   <para>The associated value must be a string.</para>
    /// </remarks>
    public const string Title = $"_DsiLayout.{nameof(Title)}";

    /// <summary>
    /// View data key for service name text.
    /// </summary>
    /// <remarks>
    ///   <para>The service name defaults to "DfE Sign-in".</para>
    ///   <para>The associated value must be a string.</para>
    /// </remarks>
    public const string ServiceName = $"_DsiLayout.{nameof(ServiceName)}";

    /// <summary>
    /// View data key for breadcrumbs.
    /// </summary>
    /// <remarks>
    ///   <para>The associated value must be assignable to <see cref="IEnumerable{CrumbViewModel}"/>.</para>
    /// </remarks>
    public const string Hero = $"_DsiLayout.{nameof(Hero)}";

    /// <summary>
    /// View data key for breadcrumbs.
    /// </summary>
    /// <remarks>
    ///   <para>The associated value must be assignable to <see cref="IEnumerable{CrumbViewModel}"/>.</para>
    /// </remarks>
    public const string Crumbs = $"_DsiLayout.{nameof(Crumbs)}";

    /// <summary>
    /// View data key for sign out URL.
    /// </summary>
    /// <remarks>
    ///   <para>The associated value must be a string or assignable to <see cref="Uri"/>.</para>
    /// </remarks>
    public const string SignOutUrl = $"_DsiLayout.{nameof(SignOutUrl)}";
}
