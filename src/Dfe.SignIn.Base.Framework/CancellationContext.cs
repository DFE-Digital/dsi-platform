namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Provides access to a <see cref="CancellationToken"/> that represents the current
/// cancellation context for an operation.
/// </summary>
/// <remarks>
///   <para>Implementations typically supply a token that can be observed by long-running
///   or asynchronous operations to cooperatively handle cancellation.</para>
/// </remarks>
public interface ICancellationContext
{
    /// <summary>
    /// Gets or sets the <see cref="CancellationToken"/> associated with the current operation.
    /// </summary>
    /// <remarks>
    ///   <para>Consumers should check <see cref="CancellationToken.IsCancellationRequested"/>
    ///   or pass the token to cancellable APIs. Setters should only be used at the beginning
    ///   of an operation or scope to establish the ambient cancellation token.</para>
    ///   <para>If you are looking to edit the cancellation token for the current context;
    ///   please consider using <see cref="CancellationContextExtensions.ScopeAsync"/> instead.</para>
    ///   <para>Updating this value adjusts the cancellation token for the current `async`
    ///   execution scope. When updating the cancellation context; please do so within an
    ///   asynchronous function that is decorated with the `async` keyword to ensure that the
    ///   token is changed for the intended asynchronous scope.</para>
    /// </remarks>
    CancellationToken CancellationToken { get; set; }
}

/// <summary>
/// Default ambient implementation of <see cref="ICancellationContext"/> using <see cref="AsyncLocal{T}"/>.
/// </summary>
/// <remarks>
///   <para>Stores the current <see cref="CancellationToken"/> in an <see cref="AsyncLocal{T}"/>
///   so the value flows with asynchronous execution contexts without requiring explicit
///   parameter passing.</para>
/// </remarks>
public sealed class CancellationContext : ICancellationContext
{
    /// <summary>
    /// Holds the current <see cref="CancellationToken"/> for the asynchronous execution context.
    /// </summary>
    /// <remarks>
    ///   <para>The value flows across async/await boundaries but does not cross thread-pool
    ///   boundaries unless captured by the logical call context. This provides an ambient way
    ///   to access cancellation in asynchronous code paths.</para>
    /// </remarks>
    private static readonly AsyncLocal<CancellationToken> CurrentCancellationToken = new();

    /// <inheritdoc/>
    public CancellationToken CancellationToken {
        get => CurrentCancellationToken.Value;
        set => CurrentCancellationToken.Value = value;
    }
}

/// <summary>
/// Extensions for <see cref="ICancellationContext"/> implementations.
/// </summary>
public static class CancellationContextExtensions
{
    /// <summary>
    /// Invokes given behaviour in a new cancellation scope.
    /// </summary>
    /// <param name="context">The cancellation context.</param>
    /// <param name="behaviour">The behaviour that is to be invoked within the new cancellation scope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="context"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="behaviour"/> is null.</para>
    /// </exception>
    public static async Task ScopeAsync(
        this ICancellationContext context, Func<Task> behaviour, CancellationToken cancellationToken)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));
        ExceptionHelpers.ThrowIfArgumentNull(behaviour, nameof(behaviour));

        cancellationToken.ThrowIfCancellationRequested();

        // This function MUST be marked with the `async` keyword since it adjust the cancellation context.
        context.CancellationToken = cancellationToken;

        await behaviour();
    }

    /// <summary>
    /// Invokes given behaviour in a new cancellation scope.
    /// </summary>
    /// <param name="context">The cancellation context.</param>
    /// <param name="behaviour">The behaviour that is to be invoked within the new cancellation scope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///   <para>Result of the behaviour.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="context"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="behaviour"/> is null.</para>
    /// </exception>
    public static async Task<TResult> ScopeAsync<TResult>(
        this ICancellationContext context, Func<Task<TResult>> behaviour, CancellationToken cancellationToken)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));
        ExceptionHelpers.ThrowIfArgumentNull(behaviour, nameof(behaviour));

        cancellationToken.ThrowIfCancellationRequested();

        // This function MUST be marked with the `async` keyword since it adjust the cancellation context.
        context.CancellationToken = cancellationToken;

        return await behaviour();
    }
}

/// <summary>
/// Marks a request type as non-cancellable, indicating that operations performed
/// by the class should not be subject to ambient cancellation.
/// </summary>
/// <remarks>
///   <para>This attribute can be used on request types to force the use of
///   <see cref="CancellationToken.None"/>.</para>
///   <para>For example, cancellation can be avoided when writing to the audit
///   since when that operation begins it should complete to ensure the audit
///   information is captured:</para>
///   <code language="csharp"><![CDATA[
///     [NonCancellable]
///     public sealed record WriteToAuditRequest
///     {
///         ...
///     }
///   ]]></code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NonCancellableAttribute : Attribute { }
