namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public class OrganisationsUnitOfWorkTests
{
    [TestMethod]
    public void OrganisationsUnitOfWork_CanBeConstructed()
    {
        var dbContext = new DbOrganisationsContext();

        var uow = new OrganisationsUnitOfWork(dbContext, new EntityFrameworkTransactionContext());

        Assert.IsNotNull(uow);
        Assert.IsInstanceOfType(uow, typeof(OrganisationsUnitOfWork));
        Assert.IsInstanceOfType(uow, typeof(EntityFrameworkUnitOfWork));
    }
}
