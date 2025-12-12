using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public sealed class UnitOfWorkTestsOrganisations
{
    [TestMethod]
    public void UnitOfWorkOrganisations_CanBeConstructed()
    {
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase("Orgs")
            .Options;

        var dbContext = new DbOrganisationsContext(options);

        var uow = new UnitOfWorkOrganisations(dbContext, new EntityFrameworkTransactionContext());

        Assert.IsNotNull(uow);
        Assert.IsInstanceOfType(uow, typeof(UnitOfWorkOrganisations));
        Assert.IsInstanceOfType(uow, typeof(EntityFrameworkUnitOfWork));
    }
}
