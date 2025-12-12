using Dfe.SignIn.Core.Interfaces.DataAccess;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// Represents a Unit of Work specifically for the <see cref="DbDirectoriesContext"/>.
/// Inherits from <see cref="EntityFrameworkUnitOfWork"/> to provide repository access and transaction
/// management for the directories database.
/// </summary>
/// <remarks>
///   <para>This class implements <see cref="IUnitOfWorkDirectories"/> as a marker interface
///   to allow dependency injection to distinguish it from other UnitOfWork implementations.
///   It is a thin wrapper around <see cref="EntityFrameworkUnitOfWork"/> and does not add
///   additional logic.</para>
/// </remarks>
public sealed class UnitOfWorkDirectories : EntityFrameworkUnitOfWork, IUnitOfWorkDirectories
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkDirectories"/> class
    /// using the specified <see cref="DbDirectoriesContext"/>.
    /// </summary>
    /// <param name="db">
    ///   The <see cref="DbDirectoriesContext"/> instance used to perform database operations.
    /// </param>
    /// <param name="transactionContext">
    ///   The Entity Framework core transaction context.
    /// </param>
    public UnitOfWorkDirectories(DbDirectoriesContext db, IEntityFrameworkTransactionContext transactionContext) : base(db, transactionContext) { }
}
