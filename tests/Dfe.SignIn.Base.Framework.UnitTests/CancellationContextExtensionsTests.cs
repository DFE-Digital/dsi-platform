namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class CancellationContextExtensionsTests
{
    #region ScopeAsync(ICancellationContext, Func<Task>, CancellationToken)

    [TestMethod]
    public async Task ScopeAsync_Throws_WhenContextArgumentIsNull()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => CancellationContextExtensions.ScopeAsync(
                context: null!,
                behaviour: () => Task.CompletedTask,
                cancellationToken: CancellationToken.None
            ));
    }

    [TestMethod]
    public async Task ScopeAsync_Throws_WhenBehaviourArgumentIsNull()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => CancellationContextExtensions.ScopeAsync(
                context: new CancellationContext(),
                behaviour: null!,
                cancellationToken: CancellationToken.None
            ));
    }

    [TestMethod]
    public async Task ScopeAsync_InvokesBehaviourWithExpectedCancellationContext()
    {
        var context = new CancellationContext();

        using var rootCancellationSource = new CancellationTokenSource();
        context.CancellationToken = rootCancellationSource.Token;

        CancellationToken? capturedCancellationToken = null;
        await CancellationContextExtensions.ScopeAsync(context, async () => {
            capturedCancellationToken = context.CancellationToken;
            await Task.CompletedTask;
        }, CancellationToken.None);

        // Ensure that correct cancellation token was set in context.
        Assert.AreEqual(CancellationToken.None, capturedCancellationToken);
        // Ensure that context is correct after behaviour has completed.
        Assert.AreEqual(rootCancellationSource.Token, context.CancellationToken);
    }

    [TestMethod]
    public async Task ScopeAsync_Throws_WhenTokenWasAlreadyCancelled()
    {
        var context = new CancellationContext {
            CancellationToken = CancellationToken.None,
        };

        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(()
            => CancellationContextExtensions.ScopeAsync(
                context,
                async () => await Task.CompletedTask,
                cancellationSource.Token
            ));
    }

    #endregion

    #region ScopeAsync_TResult(ICancellationContext, Func<Task<TResult>>, CancellationToken)

    [TestMethod]
    public async Task ScopeAsync_TResult_Throws_WhenContextArgumentIsNull()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => CancellationContextExtensions.ScopeAsync(
                context: null!,
                behaviour: async () => {
                    await Task.CompletedTask;
                    return 42;
                },
                cancellationToken: CancellationToken.None
            ));
    }

    [TestMethod]
    public async Task ScopeAsync_TResult_Throws_WhenBehaviourArgumentIsNull()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => CancellationContextExtensions.ScopeAsync<int>(
                context: new CancellationContext(),
                behaviour: null!,
                cancellationToken: CancellationToken.None
            ));
    }

    [TestMethod]
    public async Task ScopeAsync_TResult_InvokesBehaviourWithExpectedCancellationContext()
    {
        var context = new CancellationContext();

        using var rootCancellationSource = new CancellationTokenSource();
        context.CancellationToken = rootCancellationSource.Token;

        CancellationToken? capturedCancellationToken = null;
        await CancellationContextExtensions.ScopeAsync(context, async () => {
            capturedCancellationToken = context.CancellationToken;
            await Task.CompletedTask;
            return 42;
        }, CancellationToken.None);

        // Ensure that correct cancellation token was set in context.
        Assert.AreEqual(CancellationToken.None, capturedCancellationToken);
        // Ensure that context is correct after behaviour has completed.
        Assert.AreEqual(rootCancellationSource.Token, context.CancellationToken);
    }

    [TestMethod]
    public async Task ScopeAsync_TResult_ReturnsExpectedResult()
    {
        using var cancellationSource = new CancellationTokenSource();
        var context = new CancellationContext {
            CancellationToken = cancellationSource.Token,
        };

        int result = await CancellationContextExtensions.ScopeAsync(
            context,
            async () => {
                await Task.CompletedTask;
                return 42;
            },
            CancellationToken.None
        );

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ScopeAsync_TResult_Throws_WhenTokenWasAlreadyCancelled()
    {
        var context = new CancellationContext {
            CancellationToken = CancellationToken.None,
        };

        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(()
            => CancellationContextExtensions.ScopeAsync(
                context,
                async () => {
                    await Task.CompletedTask;
                    return 42;
                },
                cancellationSource.Token
            ));
    }

    #endregion
}
