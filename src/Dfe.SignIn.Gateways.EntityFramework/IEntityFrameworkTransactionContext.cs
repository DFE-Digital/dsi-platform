
namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// Defines a context for tracking transaction state in Entity Framework operations.
/// </summary>
public interface IEntityFrameworkTransactionContext
{
    /// <summary>
    /// Indicates whether the current context is inside a transaction.
    /// </summary>
    bool InsideTransaction { get; set; }
}

/// <summary>
/// Implementation of <see cref="IEntityFrameworkTransactionContext"/>
/// that internally tracks nested transaction starts using a counter.
/// </summary>
public sealed class EntityFrameworkTransactionContext : IEntityFrameworkTransactionContext
{
    private int counter = 0;

    /// <inheritdoc/>
    public bool InsideTransaction {
        get => this.counter > 0;
        set => this.counter = Math.Max(0, this.counter + (value ? 1 : -1));
    }
}
