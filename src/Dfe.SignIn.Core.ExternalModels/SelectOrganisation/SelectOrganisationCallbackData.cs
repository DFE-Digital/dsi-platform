using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// Represents callback data for a "select organisation" submission.
/// </summary>
public abstract record SelectOrganisationCallback()
{
    private static readonly Dictionary<string, Type> TypeMappings = new() {
        { PayloadTypeConstants.Error, typeof(SelectOrganisationCallbackError) },
        { PayloadTypeConstants.SignOut, typeof(SelectOrganisationCallbackSignOut) },
        { PayloadTypeConstants.Cancel, typeof(SelectOrganisationCallbackCancel) },
        { PayloadTypeConstants.Selection, typeof(SelectOrganisationCallbackSelection) },
    };

    /// <summary>
    /// Tries to resolve the specified callback payload type.
    /// </summary>
    /// <param name="payloadType">Name of the payload type (see <see cref="PayloadTypeConstants"/>).</param>
    /// <returns>
    ///   <para>The resolved type of <see cref="SelectOrganisationCallback"/>; otherwise,
    ///   a value of <c>null</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="payloadType"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="payloadType"/> is an empty string.</para>
    /// </exception>
    public static Type? TryResolveType(string payloadType)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(payloadType, nameof(payloadType));

        TypeMappings.TryGetValue(payloadType, out var result);
        return result;
    }

    /// <summary>
    /// Resolves the specified callback payload type.
    /// </summary>
    /// <param name="payloadType">Name of the payload type (see <see cref="PayloadTypeConstants"/>).</param>
    /// <returns>
    ///   <para>The resolved type of <see cref="SelectOrganisationCallback"/>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="payloadType"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="payloadType"/> is an empty string.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If type could not be resolved for <paramref name="payloadType"/>.</para>
    /// </exception>
    public static Type ResolveType(string payloadType)
    {
        return TryResolveType(payloadType)
            ?? throw new InvalidOperationException($"Cannot resolve unknown payload type '{payloadType}'.");
    }

    /// <summary>
    /// Gets the ID of the user that the selection was initiated for.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets type type of callback payload.
    /// </summary>
    [MinLength(1)]
    public required string Type { get; init; }
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when an
/// error has occurred.
/// </summary>
public sealed record SelectOrganisationCallbackError() : SelectOrganisationCallback
{
    /// <summary>
    /// Gets a value indicating the kind of error that has occurred.
    /// </summary>
    [EnumDataType(typeof(SelectOrganisationErrorCode))]
    public required SelectOrganisationErrorCode Code { get; init; }
}

/// <summary>
/// Indicates the type of error that has occurred.
/// </summary>
public enum SelectOrganisationErrorCode
{
    /// <summary>
    /// Indicates that an internal error has occurred.
    /// </summary>
    InternalError = 0,

    /// <summary>
    /// Indicates that an invalid selection was made.
    /// </summary>
    InvalidSelection = 1,

    /// <summary>
    /// Indicates that there were no options for the user to choose from.
    /// </summary>
    NoOptions = 2,
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when
/// the user has requested to sign-out.
/// </summary>
public sealed record SelectOrganisationCallbackSignOut() : SelectOrganisationCallback
{
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when
/// the user has requested to cancel selection.
/// </summary>
public sealed record SelectOrganisationCallbackCancel() : SelectOrganisationCallback
{
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when
/// the user has selected an organisation.
/// </summary>
public sealed record SelectOrganisationCallbackSelection() : SelectOrganisationCallback
{
    /// <summary>
    /// Gets the detail level of the selection.
    /// </summary>
    [EnumDataType(typeof(OrganisationDetailLevel))]
    public required OrganisationDetailLevel DetailLevel { get; init; }

    /// <summary>
    /// Gets the user selection.
    /// </summary>
    public required SelectedOrganisation Selection { get; init; }
}
