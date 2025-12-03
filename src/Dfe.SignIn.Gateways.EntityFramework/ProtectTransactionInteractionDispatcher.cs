using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// An interaction dispatcher that checks whether the current <see cref="IEntityFrameworkTransactionContext"/>
/// is inside a transaction, and throws an exception if so.  This prevents nested Entity Framework Core
/// transactions, which are not supported.
/// </summary>
/// <param name="inner"></param>
/// <param name="transactionContext"></param>
public sealed class ProtectTransactionInteractionDispatcher(
    IInteractionDispatcher inner,
    IEntityFrameworkTransactionContext transactionContext
) : IInteractionDispatcher
{
    /// <inheritdoc/>
    public InteractionTask DispatchAsync<TRequest>(InteractionContext<TRequest> context) where TRequest : class
    {
        if (transactionContext.InsideTransaction) {
            throw new InvalidOperationException("Cannot dispatch interaction within the context of a transaction.");
        }
        return inner.DispatchAsync(context);
    }
}
