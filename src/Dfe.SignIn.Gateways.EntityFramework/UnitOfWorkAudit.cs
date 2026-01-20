using Dfe.SignIn.Core.Interfaces.DataAccess;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// Represents a Unit of Work specifically for the <see cref="DbAuditContext"/>.
/// Inherits from <see cref="EntityFrameworkUnitOfWork"/> to provide repository access and transaction
/// management for the audit database.
/// </summary>
/// <remarks>
///   <para>This class implements <see cref="IUnitOfWorkAudit"/> as a marker interface
///   to allow dependency injection to distinguish it from other UnitOfWork implementations.
///   It is a thin wrapper around <see cref="EntityFrameworkUnitOfWork"/> and does not add
///   additional logic.</para>
/// </remarks>
public sealed class UnitOfWorkAudit : EntityFrameworkUnitOfWork, IUnitOfWorkAudit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkAudit"/> class
    /// using the specified <see cref="DbAuditContext"/>.
    /// </summary>
    /// <param name="db">
    ///   The <see cref="DbAuditContext"/> instance used to perform database operations.
    /// </param>
    /// <param name="transactionContext">
    ///   The Entity Framework core transaction context.
    /// </param>
    public UnitOfWorkAudit(DbAuditContext db, IEntityFrameworkTransactionContext transactionContext) : base(db, transactionContext) { }
}
