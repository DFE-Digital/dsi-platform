using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Interfaces.DataAccess;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// Represents a Unit of Work specifically for the <see cref="DbOrganisationsContext"/>.
/// Inherits from <see cref="EntityFrameworkUnitOfWork"/> to provide repository access and transaction
/// management for the organisations database.
/// </summary>
/// <remarks>
///   <para>This class implements <see cref="IUnitOfWorkOrganisations"/> as a marker interface
///   to allow dependency injection to distinguish it from other UnitOfWork implementations.
///   It is a thin wrapper around <see cref="EntityFrameworkUnitOfWork"/> and does not add
///   additional logic.</para>
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class UnitOfWorkOrganisations : EntityFrameworkUnitOfWork, IUnitOfWorkOrganisations
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkOrganisations"/> class
    /// using the specified <see cref="DbOrganisationsContext"/>.
    /// </summary>
    /// <param name="db">
    ///   The <see cref="DbOrganisationsContext"/> instance used to perform database operations.
    /// </param>
    /// <param name="transactionContext">
    ///   The Entity Framework core transaction context.
    /// </param>
    public UnitOfWorkOrganisations(DbOrganisationsContext db, IEntityFrameworkTransactionContext transactionContext) : base(db, transactionContext) { }
}
