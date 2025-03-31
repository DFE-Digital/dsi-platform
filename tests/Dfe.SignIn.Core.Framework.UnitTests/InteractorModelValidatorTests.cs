using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Framework.UnitTests.Fakes;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class InteractorModelValidatorTests
{
    private static void MockInteractorModelValidationOptions(AutoMocker autoMocker, InteractorModelValidationOptions? options = null)
    {
        autoMocker.GetMock<IOptions<InteractorModelValidationOptions>>()
            .SetupGet(x => x.Value)
            .Returns(options ?? new InteractorModelValidationOptions {
                ValidateRequestModels = true,
                ValidateResponseModels = true,
            });
    }

    #region InvokeAsync(TRequest)

    [TestMethod]
    public async Task InvokeAsync_PassesValidation()
    {
        var autoMocker = new AutoMocker();
        MockInteractorModelValidationOptions(autoMocker);

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ExampleInteractorWithValidationResponse {
                Percentage = 0.5f,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Jerry",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        });
    }

    [TestMethod]
    public async Task InvokeAsync_PassesValidation_WhenValidationIsDisabled()
    {
        var autoMocker = new AutoMocker();

        MockInteractorModelValidationOptions(autoMocker, new() {
            ValidateRequestModels = false,
            ValidateResponseModels = false,
        });

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ExampleInteractorWithValidationResponse {
                Percentage = 1.5f,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Alex",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        });
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task InvokeAsync_Throws_WhenRequestModelFailsValidation()
    {
        var autoMocker = new AutoMocker();

        MockInteractorModelValidationOptions(autoMocker, new() {
            ValidateRequestModels = true,
        });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "A",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        });
    }

    [DataRow(-1)]
    [DataRow(2)]
    [DataTestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task InvokeAsync_Throws_WhenRequestModelHasInvalidEnumValue(int badEnumValue)
    {
        var autoMocker = new AutoMocker();
        MockInteractorModelValidationOptions(autoMocker);

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ExampleInteractorWithValidationResponse {
                Percentage = 0.5f,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Jerry",
            SomeEnumProperty = (ExampleInteractorEnum)badEnumValue,
        });
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task InvokeAsync_Throws_WhenResponseModelFailsValidation()
    {
        var autoMocker = new AutoMocker();

        MockInteractorModelValidationOptions(autoMocker, new() {
            ValidateResponseModels = true,
        });

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ExampleInteractorWithValidationResponse {
                Percentage = 1.5f,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Alex",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        });
    }

    [TestMethod]
    [ExpectedException(typeof(FakeInteractionException))]
    public async Task InvokeAsync_Throws_WhenInnerInteractionThrowsInteractionException()
    {
        var autoMocker = new AutoMocker();
        MockInteractorModelValidationOptions(autoMocker);

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws(new FakeInteractionException("Fake exception."));

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Alex",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        });
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenInnerInteractionThrowsAnUnexpectedException()
    {
        var autoMocker = new AutoMocker();
        MockInteractorModelValidationOptions(autoMocker);

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws(new ValidationException("Fake exception."));

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        var exception = await Assert.ThrowsExceptionAsync<UnexpectedException>(
            () => decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
                Name = "Alex",
                SomeEnumProperty = ExampleInteractorEnum.FirstValue,
            })
        );
        Assert.IsInstanceOfType<ValidationException>(exception.InnerException);
        Assert.AreEqual("Fake exception.", exception.InnerException.Message);
        Assert.AreEqual("An unexpected exception occurred whilst processing interaction.", exception.Message);
    }

    [TestMethod]
    public async Task InvokeAsync_PassesCancellationTokenToInnerInteractor()
    {
        var autoMocker = new AutoMocker();
        MockInteractorModelValidationOptions(autoMocker);

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ExampleInteractorWithValidationResponse {
                Percentage = 0.5f,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        var expectedCancellationToken = new CancellationToken();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Alex",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        }, expectedCancellationToken);

        autoMocker.Verify<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>(
            innerInteractor => innerInteractor.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.Is<CancellationToken>(cancellationToken =>
                    cancellationToken == expectedCancellationToken
                )
            )
        );
    }

    [TestMethod]
    [ExpectedException(typeof(OperationCanceledException))]
    public async Task InvokeAsync_Throws_WhenInnerThrowsOperationCancelledException()
    {
        var autoMocker = new AutoMocker();
        MockInteractorModelValidationOptions(autoMocker);

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<ExampleInteractorWithValidationRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws(new OperationCanceledException());

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        var expectedCancellationToken = new CancellationToken();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Alex",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        }, expectedCancellationToken);
    }

    #endregion
}
