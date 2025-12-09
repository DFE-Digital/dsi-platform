namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public class UnitOfWorkTestsOrganisations
{
    [TestMethod]
    public void UnitOfWorkOrganisations_CanBeConstructed()
    {
        var dbContext = new DbOrganisationsContext();

        var uow = new OrganisationsUnitOfWork(dbContext, new EntityFrameworkTransactionContext());

        Assert.IsNotNull(uow);
        Assert.IsInstanceOfType(uow, typeof(OrganisationsUnitOfWork));
        Assert.IsInstanceOfType(uow, typeof(EntityFrameworkUnitOfWork));
    }
}
