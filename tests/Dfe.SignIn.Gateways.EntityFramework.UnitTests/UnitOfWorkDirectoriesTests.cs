using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public sealed class UnitOfWorkDirectoriesTests
{
    [TestMethod]
    public void UnitOfWorkDirectories_CanBeConstructed()
    {
        var options = new DbContextOptionsBuilder<DbDirectoriesContext>()
            .UseInMemoryDatabase("Dirs")
            .Options;

        var dbContext = new DbDirectoriesContext(options);

        var uow = new UnitOfWorkDirectories(dbContext, new EntityFrameworkTransactionContext());

        Assert.IsNotNull(uow);
        Assert.IsInstanceOfType(uow, typeof(UnitOfWorkDirectories));
        Assert.IsInstanceOfType(uow, typeof(EntityFrameworkUnitOfWork));
    }
}
