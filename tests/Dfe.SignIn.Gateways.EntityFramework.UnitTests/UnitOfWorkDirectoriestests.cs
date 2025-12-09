using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public class UnitOfWorkTestsDirectories
{
    [TestMethod]
    public void UnitOfWorkDirectories_CanBeConstructed()
    {
        var options = new DbContextOptionsBuilder<DbDirectoriesContext>()
            .UseInMemoryDatabase("Dirs")
            .Options;

        var dbContext = new DbDirectoriesContext(options);

        var uow = new DirectoriesUnitOfWork(dbContext, new EntityFrameworkTransactionContext());

        Assert.IsNotNull(uow);
        Assert.IsInstanceOfType(uow, typeof(DirectoriesUnitOfWork));
        Assert.IsInstanceOfType(uow, typeof(EntityFrameworkUnitOfWork));
    }
}
