using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.UnitTests.TestHelpers;

public static class EntityFrameworkTestHelpers
{
    public static DbDirectoriesContext UseInMemoryDirectoriesDb()
    {
        var options = new DbContextOptionsBuilder<DbDirectoriesContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbDirectoriesContext(options);

        return ctx;
    }

    public static DbOrganisationsContext UseInMemoryOrganisationsDb()
    {
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbOrganisationsContext(options);

        return ctx;
    }
}
