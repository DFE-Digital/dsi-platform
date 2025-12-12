using Dfe.SignIn.Base.Framework;
using Moq;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public sealed class ProtectTransactionInteractionDispatcherTests
{
    private Mock<IInteractionDispatcher> innerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        this.innerMock = new Mock<IInteractionDispatcher>();
    }

    [TestMethod]
    public void DispatchAsync_Throws_WhenInsideTransaction()
    {
        var transactionContext = new EntityFrameworkTransactionContext {
            InsideTransaction = true
        };

        var sut = new ProtectTransactionInteractionDispatcher(
            this.innerMock.Object,
            transactionContext
        );

        var ctx = new InteractionContext<object>(new object());

        Assert.ThrowsExactly<InvalidOperationException>(() => {
            sut.DispatchAsync(ctx);
        });

        this.innerMock.Verify(x => x.DispatchAsync(It.IsAny<InteractionContext<object>>()), Times.Never);
    }
}
