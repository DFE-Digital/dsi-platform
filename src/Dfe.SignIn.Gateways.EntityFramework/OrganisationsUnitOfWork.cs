using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// A partial class representing the Entity Framework database context for organisations.
/// </summary>
public partial class DbOrganisationsContext : DbContext { }

/// <summary>
/// Represents a Unit of Work specifically for the <see cref="DbOrganisationsContext"/>.
/// Inherits from <see cref="EntityFrameworkUnitOfWork"/> to provide repository access and transaction
/// management for the organisations database.
/// </summary>
/// <remarks>
/// This class implements <see cref="IOrganisationsUnitOfWork"/> as a marker interface
/// to allow dependency injection to distinguish it from other UnitOfWork implementations.
/// It is a thin wrapper around <see cref="EntityFrameworkUnitOfWork"/> and does not add additional logic.
/// </remarks>
public class OrganisationsUnitOfWork : EntityFrameworkUnitOfWork, IOrganisationsUnitOfWork
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrganisationsUnitOfWork"/> class
    /// using the specified <see cref="DbOrganisationsContext"/>.
    /// </summary>
    /// <param name="db">
    ///   The <see cref="DbOrganisationsContext"/> instance used to perform database operations.
    /// </param>
    /// <param name="transactionContext">
    ///   The Entity Framework core transaction context.
    /// </param>
    public OrganisationsUnitOfWork(DbOrganisationsContext db, IEntityFrameworkTransactionContext transactionContext) : base(db, transactionContext) { }
}
