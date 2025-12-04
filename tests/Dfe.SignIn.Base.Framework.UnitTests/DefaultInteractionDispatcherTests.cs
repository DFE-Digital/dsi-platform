using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework.UnitTests.Fakes;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class DefaultInteractionDispatcherTests
{
    private static Mock<IInteractor<TRequest>> SetupMockInteractor<TRequest>(AutoMocker autoMocker)
        where TRequest : class
    {
        var mockInteractor = autoMocker.GetMock<IInteractor<TRequest>>();

        autoMocker.GetMock<IInteractorResolver>()
            .Setup(x => x.ResolveInteractor<TRequest>())
            .Returns(mockInteractor.Object);

        return mockInteractor;
    }

    private static void SetupMockInteractionValidator(AutoMocker autoMocker, bool validRequest = true)
    {
        var mockValidator = autoMocker.GetMock<IInteractionValidator>();

        mockValidator
            .Setup(x => x.TryValidateRequest(
                It.IsAny<object>(),
                It.IsAny<ICollection<ValidationResult>>()
            ))
            .Returns(validRequest);
    }

    private static readonly ExampleInteractorWithValidationRequest FakeRequest = new() {
        Name = "Alex",
        SomeEnumProperty = ExampleInteractorEnum.FirstValue,
    };

    #region DispatchAsync<TRequest>(InteractionContext<TRequest>)

    [TestMethod]
    public async Task DispatchAsync_Throws_WhenContextArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        InteractionContext<ExampleInteractorWithValidationRequest> nullInteractionContext = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async ()
            => await interaction.DispatchAsync(nullInteractionContext));
    }

    [TestMethod]
    public async Task DispatchAsync_Throws_WhenInteractorCannotBeResolved()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleInteractorWithValidationRequest>(FakeRequest);

        var exception = await Assert.ThrowsExactlyAsync<MissingInteractorException>(async ()
            => await interaction.DispatchAsync(interactionContext));
        Assert.AreEqual("Cannot resolve interactor for request type 'Dfe.SignIn.Base.Framework.UnitTests.Fakes.ExampleInteractorWithValidationRequest'.", exception.Message);
        Assert.AreEqual("Dfe.SignIn.Base.Framework.UnitTests.Fakes.ExampleInteractorWithValidationRequest", exception.RequestType);
    }

    [TestMethod]
    public async Task DispatchAsync_ValidatesRequestModel()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        SetupMockInteractor<ExampleInteractorWithValidationRequest>(autoMocker)
            .Setup(x => x.InvokeAsync(
                It.IsAny<InteractionContext<ExampleInteractorWithValidationRequest>>(),
                It.IsAny<CancellationToken>()
            ));

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleInteractorWithValidationRequest>(FakeRequest);

        await interaction.DispatchAsync(interactionContext);

        autoMocker.Verify<IInteractionValidator, bool>(
            x => x.TryValidateRequest(
                It.Is<ExampleInteractorWithValidationRequest>(req => ReferenceEquals(req, FakeRequest)),
                It.IsAny<ICollection<ValidationResult>>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task DispatchAsync_DoesNotThrow_WhenRequestModelIsInvalid()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker, validRequest: false);

        SetupMockInteractor<ExampleInteractorWithValidationRequest>(autoMocker)
            .Setup(x => x.InvokeAsync(
                It.IsAny<InteractionContext<ExampleInteractorWithValidationRequest>>(),
                It.IsAny<CancellationToken>()
            ));

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleInteractorWithValidationRequest>(FakeRequest);

        await interaction.DispatchAsync(interactionContext);
    }

    [TestMethod]
    public async Task DispatchAsync_InvokesExpectedInteractor()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        SetupMockInteractor<ExampleInteractorWithValidationRequest>(autoMocker)
            .Setup(x => x.InvokeAsync(
                It.IsAny<InteractionContext<ExampleInteractorWithValidationRequest>>(),
                It.IsAny<CancellationToken>()
            ));

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleInteractorWithValidationRequest>(FakeRequest);

        await interaction.DispatchAsync(interactionContext);

        autoMocker.Verify<IInteractor<ExampleInteractorWithValidationRequest>>(
            x => x.InvokeAsync(
                It.Is<InteractionContext<ExampleInteractorWithValidationRequest>>(context => ReferenceEquals(context.Request, FakeRequest)),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    [DataRow(typeof(InvalidRequestException))]
    [DataRow(typeof(InvalidResponseException))]
    public async Task DispatchAsync_ThrowsUnexpectedException_WhenInvocationIdOfValidationExceptionDoesNotMatchContext(
        Type exceptionTypeThrown)
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        SetupMockInteractor<ExampleInteractorWithValidationRequest>(autoMocker)
            .Setup(x => x.InvokeAsync(
                It.IsAny<InteractionContext<ExampleInteractorWithValidationRequest>>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback((InteractionContext<ExampleInteractorWithValidationRequest> context, CancellationToken cancellationToken) => {
                throw (Exception)Activator.CreateInstance(
                    exceptionTypeThrown,
                    new Guid("0573d250-cc6e-42d8-b458-ed7430dd85fc"),
                    Array.Empty<ValidationResult>()
                )!;
            });

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleInteractorWithValidationRequest>(FakeRequest);

        await Assert.ThrowsExactlyAsync<UnexpectedException>(async ()
            => await interaction.DispatchAsync(interactionContext));
    }

    [TestMethod]
    [DataRow(typeof(InvalidOperationException), typeof(UnexpectedException))]
    [DataRow(typeof(FakeInteractionException), typeof(FakeInteractionException))]
    [DataRow(typeof(OperationCanceledException), typeof(OperationCanceledException))]
    public async Task DispatchAsync_ThrowsCorrectExceptionType_WhenInteractionThrows(
        Type exceptionTypeThrown,
        Type expectedExceptionType)
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        SetupMockInteractor<ExampleInteractorWithValidationRequest>(autoMocker)
            .Setup(x => x.InvokeAsync(
                It.IsAny<InteractionContext<ExampleInteractorWithValidationRequest>>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws((Exception)Activator.CreateInstance(exceptionTypeThrown)!);

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleInteractorWithValidationRequest>(FakeRequest);

        var exception = await Assert.ThrowsAsync<Exception>(async ()
            => await interaction.DispatchAsync(interactionContext))
;
        Assert.IsInstanceOfType(exception, expectedExceptionType);
    }

    [TestMethod]
    public async Task DispatchAsync_Throws_WhenCancellationWasAlreadyRequested()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        SetupMockInteractor<ExampleInteractorWithValidationRequest>(autoMocker)
            .Setup(x => x.InvokeAsync(
                It.IsAny<InteractionContext<ExampleInteractorWithValidationRequest>>(),
                It.IsAny<CancellationToken>()
            ));

        var cancellationContext = new CancellationContext();
        autoMocker.Use<ICancellationContext>(cancellationContext);

        using var cancellationSource = new CancellationTokenSource();
        cancellationContext.CancellationToken = cancellationSource.Token;
        await cancellationSource.CancelAsync();

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleInteractorWithValidationRequest>(FakeRequest);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async ()
            => await interaction.DispatchAsync(interactionContext));
    }

    [TestMethod]
    public async Task DispatchAsync_UpdatesInteractionContextWithCancellationToken()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        SetupMockInteractor<ExampleInteractorWithValidationRequest>(autoMocker)
            .Setup(x => x.InvokeAsync(
                It.IsAny<InteractionContext<ExampleInteractorWithValidationRequest>>(),
                It.IsAny<CancellationToken>()
            ));

        var cancellationContext = new CancellationContext();
        autoMocker.Use<ICancellationContext>(cancellationContext);

        using var rootCancellationSource = new CancellationTokenSource();
        cancellationContext.CancellationToken = rootCancellationSource.Token;

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleInteractorWithValidationRequest>(FakeRequest);
        using var nestedCancellationSource = new CancellationTokenSource();
        interactionContext.CancellationToken = nestedCancellationSource.Token;

        await interaction.DispatchAsync(interactionContext);

        Assert.AreEqual(nestedCancellationSource.Token, interactionContext.CancellationToken);

        // Ensure that context is correct after interaction has completed.
        Assert.AreEqual(rootCancellationSource.Token, cancellationContext.CancellationToken);
    }

    [NonCancellable]
    public sealed record NonCancellableRequest { }
    public sealed record NonCancellableResponse { }

    [TestMethod]
    public async Task DispatchAsync_DoesNotThrow_WhenNonCancellableAndCancelationRequested()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        SetupMockInteractor<NonCancellableRequest>(autoMocker)
            .Setup(x => x.InvokeAsync(
                It.IsAny<InteractionContext<NonCancellableRequest>>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new NonCancellableResponse());

        var cancellationContext = new CancellationContext();
        autoMocker.Use<ICancellationContext>(cancellationContext);

        using var rootCancellationSource = new CancellationTokenSource();
        cancellationContext.CancellationToken = rootCancellationSource.Token;
        await rootCancellationSource.CancelAsync();

        var interaction = autoMocker.CreateInstance<DefaultInteractionDispatcher>();
        var interactionContext = new InteractionContext<NonCancellableRequest>(new());

        var response = await interaction.DispatchAsync(interactionContext);

        Assert.IsInstanceOfType<NonCancellableResponse>(response);

        // Ensure that context is correct after interaction has completed.
        Assert.AreEqual(rootCancellationSource.Token, cancellationContext.CancellationToken);
    }

    #endregion
}
