using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Framework.UnitTests.Fakes;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class ServiceProviderInteractionDispatcherTests
{
    private static Mock<IInteractor<TRequest>> SetupMockInteractor<TRequest>(AutoMocker autoMocker)
        where TRequest : class
    {
        var mockInteractor = autoMocker.GetMock<IInteractor<TRequest>>();

        var mockServiceProvider = autoMocker.GetMock<IServiceProvider>();
        mockServiceProvider
            .Setup(x => x.GetService(It.Is<Type>(type => type == typeof(IInteractor<TRequest>))))
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

    #region DispatchAsync<TRequest>(TRequest, CancellationToken)

    [TestMethod]
    public async Task DispatchAsync_Throws_WhenInteractorCannotBeResolved()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidator(autoMocker);

        var interaction = autoMocker.CreateInstance<ServiceProviderInteractionDispatcher>();

        async Task Act()
        {
            await interaction.DispatchAsync(FakeRequest);
        }

        var exception = await Assert.ThrowsAsync<MissingInteractorException>(Act);
        Assert.AreEqual(nameof(ExampleInteractorWithValidationRequest), exception.RequestType);
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

        var interaction = autoMocker.CreateInstance<ServiceProviderInteractionDispatcher>();

        await interaction.DispatchAsync(FakeRequest);

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

        var interaction = autoMocker.CreateInstance<ServiceProviderInteractionDispatcher>();

        await interaction.DispatchAsync(FakeRequest);
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

        var interaction = autoMocker.CreateInstance<ServiceProviderInteractionDispatcher>();

        await interaction.DispatchAsync(FakeRequest);

        autoMocker.Verify<IInteractor<ExampleInteractorWithValidationRequest>>(
            x => x.InvokeAsync(
                It.Is<InteractionContext<ExampleInteractorWithValidationRequest>>(context => ReferenceEquals(context.Request, FakeRequest)),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [DataTestMethod]
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

        var interaction = autoMocker.CreateInstance<ServiceProviderInteractionDispatcher>();

        async Task Act()
        {
            await interaction.DispatchAsync(FakeRequest);
        }

        var exception = await Assert.ThrowsAsync<Exception>(Act);
        Assert.IsInstanceOfType<UnexpectedException>(exception);
    }

    [DataTestMethod]
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

        var interaction = autoMocker.CreateInstance<ServiceProviderInteractionDispatcher>();

        async Task Act()
        {
            await interaction.DispatchAsync(FakeRequest);
        }

        var exception = await Assert.ThrowsAsync<Exception>(Act);
        Assert.IsInstanceOfType(exception, expectedExceptionType);
    }

    #endregion
}
