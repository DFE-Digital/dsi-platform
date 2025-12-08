using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests;

[TestClass]
public sealed class ControllerInteractionExtensionsTests
{
    private sealed class FakeViewModel
    {
        [MapTo<FakeRequest>(nameof(FakeRequest.RequestPropertyA))]
        public string? ViewModelPropertyA { get; set; }

        [MapTo<FakeRequest>(nameof(FakeRequest.RequestPropertyB))]
        public string? ViewModelPropertyB { get; set; }

        [MapTo<FakeRequest>(nameof(FakeRequest.RequestPropertyC), RequestMappingOptions.ValidationErrors)]
        public string? ViewModelPropertyC { get; set; }
    }

    private sealed record FakeRequest
    {
#pragma warning disable S3459
        public required string RequestPropertyA { get; set; }
#pragma warning restore S3459
        public required int RequestPropertyB { get; set; }
        public string RequestPropertyC { get; set; } = "hello";
    }

    private sealed record FakeResponse
    {
    }

    private sealed class FakeComplexViewModel
    {
        [MapTo<FakeComplexRequest>(nameof(FakeComplexRequest.RequestPropertyA))]
        public ComplexType? ViewModelPropertyA { get; set; }
    }

    private sealed record FakeComplexRequest
    {
        public ComplexType? RequestPropertyA { get; init; } = null;
    }

    private sealed record ComplexType
    {
        public NestedType? Nested { get; init; }
    }

    private sealed record NestedType
    {
        public required int Foo { get; init; }
    }

    #region MapRequestFromViewModel<TRequest>(IInteractionDispatcher, Controller, object)

    [TestMethod]
    public void MapRequestFromViewModel_Throws_WhenInteractionArgumentIsNull()
    {
        var mockController = new Mock<Controller>();
        var fakeViewModel = new FakeViewModel();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapRequestFromViewModel<FakeRequest>(
                interaction: null!,
                controller: mockController.Object,
                viewModel: fakeViewModel
            ));
    }

    [TestMethod]
    public void MapRequestFromViewModel_Throws_WhenControllerArgumentIsNull()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();
        var fakeViewModel = new FakeViewModel();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapRequestFromViewModel<FakeRequest>(
                interaction: mockInteraction.Object,
                controller: null!,
                viewModel: fakeViewModel
            ));
    }

    [TestMethod]
    public void MapRequestFromViewModel_Throws_WhenViewModelArgumentIsNull()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();
        var mockController = new Mock<Controller>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapRequestFromViewModel<FakeRequest>(
                interaction: mockInteraction.Object,
                controller: mockController.Object,
                viewModel: null!
            ));
    }

    [TestMethod]
    public async Task MapRequestFromViewModel_MapsPropertiesAsExpected()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        FakeRequest? capturedRequest = null;
        autoMocker.CaptureRequest<FakeRequest>(req => capturedRequest = req);

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync();

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("abc", capturedRequest.RequestPropertyA, "Property of same type");
        Assert.AreEqual(123, capturedRequest.RequestPropertyB, "Converts to property of different type");
        Assert.AreEqual("hello", capturedRequest.RequestPropertyC, "Does not map property when value flag not set");
    }

    [TestMethod]
    public async Task MapRequestFromViewModel_MapsNullProperties()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        FakeComplexRequest? capturedRequest = null;
        autoMocker.CaptureRequest<FakeComplexRequest>(req => capturedRequest = req);

        var fakeViewModel = new FakeComplexViewModel {
            ViewModelPropertyA = null,
        };

        await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeComplexRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync();

        Assert.IsNotNull(capturedRequest);
        Assert.IsNull(capturedRequest.RequestPropertyA);
    }

    [TestMethod]
    public async Task MapRequestFromViewModel_MapsComplexProperty()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        FakeComplexRequest? capturedRequest = null;
        autoMocker.CaptureRequest<FakeComplexRequest>(req => capturedRequest = req);

        var fakeViewModel = new FakeComplexViewModel {
            ViewModelPropertyA = new() {
                Nested = new() { Foo = 123 },
            },
        };

        await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeComplexRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync();

        Assert.IsNotNull(capturedRequest?.RequestPropertyA?.Nested);
        Assert.AreEqual(123, capturedRequest.RequestPropertyA.Nested.Foo);
    }

    #endregion

    #region MapRequestValidationResults<TRequest>(Controller, Type, IEnumerable<ValidationResult>)

    [TestMethod]
    public void MapRequestValidationResults_TRequest_Throws_WhenControllerArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapRequestValidationResults<FakeRequest>(null!, typeof(FakeViewModel), []));
    }

    [TestMethod]
    public void MapRequestValidationResults_TRequest_Throws_WhenViewModelTypeArgumentIsNull()
    {
        var mockController = new Mock<Controller>();
        Type nullViewModelType = null!;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapRequestValidationResults<FakeRequest>(mockController.Object, nullViewModelType, []));
    }

    [TestMethod]
    public void MapRequestValidationResults_TRequest_MapsErrorsToViewModel()
    {
        var mockController = new Mock<Controller>();

        var validationResults = new ValidationResult[] {
            new("Example error 1", [nameof(FakeRequest.RequestPropertyA)]),
            new("Example error 2", [nameof(FakeRequest.RequestPropertyB)]),
        };

        ControllerInteractionExtensions.MapRequestValidationResults<FakeRequest>(mockController.Object, typeof(FakeViewModel), validationResults);

        var modelState = mockController.Object.ModelState;
        Assert.IsFalse(modelState.IsValid);
        Assert.AreEqual(2, modelState.ErrorCount);
        Assert.AreEqual("Example error 1", modelState.First(x => x.Key == nameof(FakeViewModel.ViewModelPropertyA)).Value!.Errors[0].ErrorMessage);
        Assert.AreEqual("Example error 2", modelState.First(x => x.Key == nameof(FakeViewModel.ViewModelPropertyB)).Value!.Errors[0].ErrorMessage);
    }

    #endregion

    #region MapRequestValidationResults<TRequest>(Controller, object, IEnumerable<ValidationResult>)

    [TestMethod]
    public void MapRequestValidationResults_Throws_WhenControllerArgumentIsNull()
    {
        var fakeViewModel = new FakeViewModel();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapRequestValidationResults<FakeRequest>(null!, fakeViewModel, []));
    }

    [TestMethod]
    public void MapRequestValidationResults_Throws_WhenViewModelArgumentIsNull()
    {
        var mockController = new Mock<Controller>();
        object nullViewModel = null!;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapRequestValidationResults<FakeRequest>(mockController.Object, nullViewModel, []));
    }

    [TestMethod]
    public void MapRequestValidationResults_MapsErrorsToViewModel()
    {
        var mockController = new Mock<Controller>();
        var fakeViewModel = new FakeViewModel();

        var validationResults = new ValidationResult[] {
            new("Example error 1", [nameof(FakeRequest.RequestPropertyA)]),
            new("Example error 2", [nameof(FakeRequest.RequestPropertyB)]),
        };

        ControllerInteractionExtensions.MapRequestValidationResults<FakeRequest>(mockController.Object, fakeViewModel, validationResults);

        var modelState = mockController.Object.ModelState;
        Assert.IsFalse(modelState.IsValid);
        Assert.AreEqual(2, modelState.ErrorCount);
        Assert.AreEqual("Example error 1", modelState.First(x => x.Key == nameof(FakeViewModel.ViewModelPropertyA)).Value!.Errors[0].ErrorMessage);
        Assert.AreEqual("Example error 2", modelState.First(x => x.Key == nameof(FakeViewModel.ViewModelPropertyB)).Value!.Errors[0].ErrorMessage);
    }

    #endregion

    #region WithCancellation(CancellationToken)

    [TestMethod]
    public async Task WithCancellation_UpdatesInteractionContext()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        using var cancellationSource = new CancellationTokenSource();

        InteractionContext<FakeRequest>? capturedInteractionContext = null;
        autoMocker.CaptureInteractionContext<FakeRequest>(ctx => capturedInteractionContext = ctx);

        await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .WithCancellation(cancellationSource.Token)
            .DispatchAsync();

        Assert.IsNotNull(capturedInteractionContext);
        Assert.AreEqual(cancellationSource.Token, capturedInteractionContext.CancellationToken);
    }

    #endregion

    #region IgnoreCache(bool)

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task IgnoreCache_UpdatesInteractionContext(bool hintIgnore)
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        using var cancellationSource = new CancellationTokenSource();

        InteractionContext<FakeRequest>? capturedInteractionContext = null;
        autoMocker.CaptureInteractionContext<FakeRequest>(ctx => capturedInteractionContext = ctx);

        await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .IgnoreCache(hintIgnore)
            .DispatchAsync();

        Assert.IsNotNull(capturedInteractionContext);
        Assert.AreEqual(hintIgnore, capturedInteractionContext.IgnoreCacheHint);
    }

    #endregion

    #region Use(Func<TRequest, TRequest>)

    [TestMethod]
    public void Use_Throws_WhenOverrideDelegateArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
            ViewModelPropertyC = "ignored",
        };

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .Use(null!));
    }

    [TestMethod]
    public async Task Use_OverridesExpectedValues()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        FakeRequest? capturedRequest = null;
        autoMocker.CaptureRequest<FakeRequest>(req => capturedRequest = req);

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .Use(request => request with {
                RequestPropertyB = 456,
                RequestPropertyC = "overridden",
            })
            .DispatchAsync();

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("abc", capturedRequest.RequestPropertyA, "Original property value");
        Assert.AreEqual(456, capturedRequest.RequestPropertyB, "Overrides value of mapped property");
        Assert.AreEqual("overridden", capturedRequest.RequestPropertyC, "Overrides value of unmapped property");
    }

    #endregion

    #region DispatchAsync()

    [TestMethod]
    public async Task DispatchAsync_TResponse_ReturnsNull_WhenHasValidationResults()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        autoMocker.MockValidationError<FakeRequest>(nameof(FakeRequest.RequestPropertyA));

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var response = await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync();

        Assert.IsNull(response);
    }

    [TestMethod]
    public async Task DispatchAsync_TResponse_ReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        var fakeResponse = new FakeResponse();
        autoMocker.MockResponse<FakeRequest>(fakeResponse);

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var response = await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync();

        Assert.AreSame(fakeResponse, response);
    }

    [TestMethod]
    public async Task DispatchAsync_ThrowsOtherExceptions()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        var fakeException = new UnexpectedException();
        autoMocker.MockThrows<FakeRequest>(fakeException);

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var exception = await Assert.ThrowsExactlyAsync<UnexpectedException>(async ()
            => await ControllerInteractionExtensions.MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync());

        Assert.AreSame(fakeException, exception);
    }

    [TestMethod]
    public async Task DispatchAsync_ThrowsInvalidRequestException_WhenNoValidationResults()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        var fakeException = new InvalidRequestException();
        autoMocker.MockThrows<FakeRequest>(fakeException);

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await ControllerInteractionExtensions.MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync());

        Assert.AreSame(fakeException, exception);
    }

    [TestMethod]
    public async Task DispatchAsync_MapsErrorsToViewModel_WhenHasValidationResults()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        autoMocker.MockThrows<FakeRequest>(new InvalidRequestException(Guid.Empty, [
            new("Invalid value A", [nameof(FakeRequest.RequestPropertyA)]),
            new("Invalid value B", [nameof(FakeRequest.RequestPropertyB)]),
        ]));

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        await ControllerInteractionExtensions
            .MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync();

        var modelState = mockController.Object.ModelState;
        Assert.IsFalse(modelState.IsValid);
        Assert.AreEqual(2, modelState.ErrorCount);
        Assert.AreEqual("Invalid value A", modelState.First(x => x.Key == nameof(FakeViewModel.ViewModelPropertyA)).Value!.Errors[0].ErrorMessage);
        Assert.AreEqual("Invalid value B", modelState.First(x => x.Key == nameof(FakeViewModel.ViewModelPropertyB)).Value!.Errors[0].ErrorMessage);
    }

    [TestMethod]
    public async Task DispatchAsync_Throws_WhenHasUnmappedProperties()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();

        autoMocker.MockValidationError<FakeRequest>("UnmappedProperty");

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async ()
            => await ControllerInteractionExtensions.MapRequestFromViewModel<FakeRequest>(
                autoMocker.Get<IInteractionDispatcher>(),
                mockController.Object,
                fakeViewModel
            )
            .DispatchAsync());

        Assert.AreEqual("Unable to map validation result 'UnmappedProperty' with message 'Invalid value'.", exception.Message);
    }

    #endregion
}
