namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// An abstract class representing the attributes that make up a NavMenuItem
/// </summary>
public abstract record NavigationItemViewModel
{
    /// <summary>
    /// Where does the link navigate the user to
    /// </summary>
    public required Uri Href { get; init; }

    /// <summary>
    /// Human readable display text
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Is the link currently active
    /// </summary>
    public bool IsActive { get; init; }
}
/// <summary>
/// An implementation of the NavigationItemViewModel
/// </summary>
public sealed record StandardNavigationItemViewModel
    : NavigationItemViewModel;

/// <summary>
/// An implementation of the NavigationViewModel which provides a Count property
/// </summary>
public sealed record CountNavigationItemViewModel
    : NavigationItemViewModel
{
    /// <summary>
    /// A count
    /// </summary>
    public required int Count { get; init; }
}
