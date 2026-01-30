using Dfe.SignIn.Core.Interfaces.DataAccess;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.TestHelpers;

public static class EntityFrameworkTestHelpers
{
    public static DbDirectoriesContext UseInMemoryDirectoriesDb(this AutoMocker autoMocker)
    {
        var options = new DbContextOptionsBuilder<DbDirectoriesContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbDirectoriesContext(options);

        autoMocker.Use<IUnitOfWorkDirectories>(
            new UnitOfWorkDirectories(ctx, new EntityFrameworkTransactionContext())
        );

        return ctx;
    }

    public static DbOrganisationsContext UseInMemoryOrganisationsDb(this AutoMocker autoMocker)
    {
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbOrganisationsContext(options);

        autoMocker.Use<IUnitOfWorkOrganisations>(
            new UnitOfWorkOrganisations(ctx, new EntityFrameworkTransactionContext())
        );

        return ctx;
    }
}
