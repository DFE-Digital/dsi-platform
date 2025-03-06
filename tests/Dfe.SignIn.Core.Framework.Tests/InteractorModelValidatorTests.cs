using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Framework.Tests.Fakes;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.Framework.Tests;

[TestClass]
public sealed class InteractorModelValidatorTests
{
    #region InvokeAsync(TRequest)

    [TestMethod]
    public async Task InvokeAsync_PassesValidation()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IOptions<InteractorModelValidationOptions>>()
            .SetupGet(x => x.Value)
            .Returns(new InteractorModelValidationOptions {
                ValidateRequestModels = true,
                ValidateResponseModels = true,
            });

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(It.IsAny<ExampleInteractorWithValidationRequest>()))
            .ReturnsAsync(new ExampleInteractorWithValidationResponse {
                Percentage = 0.5f,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Jerry",
        });
    }

    [TestMethod]
    public async Task InvokeAsync_PassesValidation_WhenValidationIsDisabled()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(It.IsAny<ExampleInteractorWithValidationRequest>()))
            .ReturnsAsync(new ExampleInteractorWithValidationResponse {
                Percentage = 1.5f,
            });

        autoMocker.GetMock<IOptions<InteractorModelValidationOptions>>()
            .SetupGet(x => x.Value)
            .Returns(new InteractorModelValidationOptions {
                ValidateRequestModels = false,
                ValidateResponseModels = false,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "Alex",
        });
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenRequestModelFailsValidation()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IOptions<InteractorModelValidationOptions>>()
            .SetupGet(x => x.Value)
            .Returns(new InteractorModelValidationOptions {
                ValidateRequestModels = true,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await Assert.ThrowsExceptionAsync<ValidationException>(
            () => decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
                Name = "A",
            })
        );
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenResponseModelFailsValidation()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IOptions<InteractorModelValidationOptions>>()
            .SetupGet(x => x.Value)
            .Returns(new InteractorModelValidationOptions {
                ValidateResponseModels = true,
            });

        autoMocker.GetMock<IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>>()
            .Setup(x => x.InvokeAsync(It.IsAny<ExampleInteractorWithValidationRequest>()))
            .ReturnsAsync(new ExampleInteractorWithValidationResponse {
                Percentage = 1.5f,
            });

        var decorator = autoMocker.CreateInstance<
            InteractorModelValidator<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        await Assert.ThrowsExceptionAsync<ValidationException>(
            () => decorator.InvokeAsync(new ExampleInteractorWithValidationRequest {
                Name = "Alex",
            })
        );
    }

    #endregion
}
