namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests;

[TestClass]
public class EntityFrameworkTransactionContextTests
{
    [TestMethod]
    public void InsideTransaction_ShouldBeFalse_Initially()
    {
        var ctx = new EntityFrameworkTransactionContext();

        Assert.IsFalse(ctx.InsideTransaction);
    }

    [TestMethod]
    public void InsideTransaction_SetTrue_ShouldMakeInsideTransactionTrue()
    {
        var ctx = new EntityFrameworkTransactionContext {
            InsideTransaction = true
        };

        Assert.IsTrue(ctx.InsideTransaction);
    }

    [TestMethod]
    public void InsideTransaction_SetTrueTwice_ShouldRemainTrue()
    {
        var ctx = new EntityFrameworkTransactionContext {
            InsideTransaction = true
        };

        ctx.InsideTransaction = true;

        Assert.IsTrue(ctx.InsideTransaction);
    }

    [TestMethod]
    public void InsideTransaction_SetTrueTwiceAndFalseOnce_ShouldRemainTrue()
    {
        var ctx = new EntityFrameworkTransactionContext {
            InsideTransaction = true
        };
        ctx.InsideTransaction = true;
        ctx.InsideTransaction = false;

        Assert.IsTrue(ctx.InsideTransaction);
    }

    [TestMethod]
    public void InsideTransaction_SetTrueThenFalse_ShouldBeFalse()
    {
        var ctx = new EntityFrameworkTransactionContext {
            InsideTransaction = true
        };
        ctx.InsideTransaction = false;

        Assert.IsFalse(ctx.InsideTransaction);
    }

    [TestMethod]
    public void InsideTransaction_SetFalseBelowZero_ShouldReturnFalse()
    {
        var ctx = new EntityFrameworkTransactionContext {
            InsideTransaction = false
        };

        Assert.IsFalse(ctx.InsideTransaction);
    }

    [TestMethod]
    public void NestedIncrementAndDecrementSequence_ShouldBehaveCorrectly()
    {
        var ctx = new EntityFrameworkTransactionContext {
            InsideTransaction = true
        };
        ctx.InsideTransaction = true;
        ctx.InsideTransaction = false;
        ctx.InsideTransaction = false;
        ctx.InsideTransaction = false;

        Assert.IsFalse(ctx.InsideTransaction);
    }
}
