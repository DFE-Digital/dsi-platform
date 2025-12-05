namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class CancellationContextTests
{
    #region Property: CancellationToken

    [TestMethod]
    public void CancellationToken_IsNoneByDefault()
    {
        var context = new CancellationContext();

        Assert.AreEqual(CancellationToken.None, context.CancellationToken);
    }

    [TestMethod]
    public void CancellationToken_TakesOnGivenValue()
    {
        var context = new CancellationContext();

        using var cancellationSource = new CancellationTokenSource();
        context.CancellationToken = cancellationSource.Token;

        Assert.AreEqual(cancellationSource.Token, context.CancellationToken);
    }

    [TestMethod]
    public async Task CancellationToken_MaintainsExpectedTokenAtEachScope()
    {
        var context = new CancellationContext();

        using var rootCancellationSource = new CancellationTokenSource();
        context.CancellationToken = rootCancellationSource.Token;

        async Task InnerScopeA()
        {
            context.CancellationToken = CancellationToken.None;
            await InnerScopeB();
            Assert.AreEqual(CancellationToken.None, context.CancellationToken);
        }
        async Task InnerScopeB()
        {
            using var innerCancellationSource = new CancellationTokenSource();
            context.CancellationToken = innerCancellationSource.Token;
            await InnerScopeC();
            Assert.AreEqual(innerCancellationSource.Token, context.CancellationToken);
        }
        Task InnerScopeC()
        {
            Assert.AreNotEqual(CancellationToken.None, context.CancellationToken);
            return Task.CompletedTask;
        }

        await InnerScopeA();

        // Ensure that context is correct after behaviour has completed.
        Assert.AreEqual(rootCancellationSource.Token, context.CancellationToken);
    }

    #endregion
}
