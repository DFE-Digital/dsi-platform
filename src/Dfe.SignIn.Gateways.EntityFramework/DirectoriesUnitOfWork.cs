using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// A partial class representing the Entity Framework database context for organisations.
/// </summary>
public partial class DbDirectoriesContext : DbContext { }

/// <summary>
/// Represents a Unit of Work specifically for the <see cref="DbDirectoriesContext"/>.
/// Inherits from <see cref="EntityFrameworkUnitOfWork"/> to provide repository access and transaction
/// management for the directories database.
/// </summary>
/// <remarks>
/// This class implements <see cref="IDirectoriesUnitOfWork"/> as a marker interface
/// to allow dependency injection to distinguish it from other UnitOfWork implementations.
/// It is a thin wrapper around <see cref="EntityFrameworkUnitOfWork"/> and does not add additional logic.
/// </remarks>
public class DirectoriesUnitOfWork : EntityFrameworkUnitOfWork, IDirectoriesUnitOfWork
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoriesUnitOfWork"/> class
    /// using the specified <see cref="DbDirectoriesContext"/>.
    /// </summary>
    /// <param name="db">
    ///   The <see cref="DbDirectoriesContext"/> instance used to perform database operations.
    /// </param>
    /// <param name="transactionContext">
    ///   The Entity Framework core transaction context.
    /// </param>
    public DirectoriesUnitOfWork(DbDirectoriesContext db, IEntityFrameworkTransactionContext transactionContext) : base(db, transactionContext) { }
}
