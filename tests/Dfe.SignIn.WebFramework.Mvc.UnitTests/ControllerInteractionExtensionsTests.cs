using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;

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

    #region MapInteractionRequest<TRequest>(Controller, object)

    [TestMethod]
    public void MapInteractionRequest_Throws_WhenControllerArgumentIsNull()
    {
        var fakeViewModel = new FakeViewModel();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(null!, fakeViewModel));
    }

    [TestMethod]
    public void MapInteractionRequest_Throws_WhenViewModelArgumentIsNull()
    {
        var mockController = new Mock<Controller>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, null!));
    }

    [TestMethod]
    public async Task MapInteractionRequest_MapsPropertiesAsExpected()
    {
        var mockController = new Mock<Controller>();

        FakeRequest? capturedRequest = null;
        InteractionTask capturer(FakeRequest request, CancellationToken _)
        {
            capturedRequest = request;
            return InteractionTask.FromResult(new FakeResponse());
        }

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        await ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel)
            .InvokeAsync(capturer, CancellationToken.None);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("abc", capturedRequest.RequestPropertyA, "Property of same type");
        Assert.AreEqual(123, capturedRequest.RequestPropertyB, "Converts to property of different type");
        Assert.AreEqual("hello", capturedRequest.RequestPropertyC, "Does not map property when value flag not set");
    }

    [TestMethod]
    public async Task MapInteractionRequest_MapsNullProperties()
    {
        var mockController = new Mock<Controller>();

        FakeComplexRequest? capturedRequest = null;
        InteractionTask capturer(FakeComplexRequest request, CancellationToken _)
        {
            capturedRequest = request;
            return InteractionTask.FromResult(new FakeResponse());
        }

        var fakeViewModel = new FakeComplexViewModel {
            ViewModelPropertyA = null,
        };

        await ControllerInteractionExtensions.MapInteractionRequest<FakeComplexRequest>(mockController.Object, fakeViewModel)
            .InvokeAsync(capturer, CancellationToken.None);

        Assert.IsNotNull(capturedRequest);
        Assert.IsNull(capturedRequest.RequestPropertyA);
    }

    [TestMethod]
    public async Task MapInteractionRequest_MapsComplexProperty()
    {
        var mockController = new Mock<Controller>();

        FakeComplexRequest? capturedRequest = null;
        InteractionTask capturer(FakeComplexRequest request, CancellationToken _)
        {
            capturedRequest = request;
            return InteractionTask.FromResult(new FakeResponse());
        }

        var fakeViewModel = new FakeComplexViewModel {
            ViewModelPropertyA = new() {
                Nested = new() { Foo = 123 },
            },
        };

        await ControllerInteractionExtensions.MapInteractionRequest<FakeComplexRequest>(mockController.Object, fakeViewModel)
            .InvokeAsync(capturer, CancellationToken.None);

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

    #region Use(Func<TRequest, TRequest>)

    [TestMethod]
    public void Use_Throws_WhenOverrideDelegateArgumentIsNull()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
            ViewModelPropertyC = "ignored",
        };

        var builder = ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel);

        Assert.ThrowsExactly<ArgumentNullException>(()
            => builder.Use(null!));
    }

    [TestMethod]
    public async Task Use_OverridesExpectedValues()
    {
        var mockController = new Mock<Controller>();

        FakeRequest? capturedRequest = null;
        InteractionTask capturer(FakeRequest request, CancellationToken _)
        {
            capturedRequest = request;
            return InteractionTask.FromResult(new FakeResponse());
        }

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        await ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel)
            .Use(request => request with {
                RequestPropertyB = 456,
                RequestPropertyC = "overridden",
            })
            .InvokeAsync(capturer, CancellationToken.None);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("abc", capturedRequest.RequestPropertyA, "Original property value");
        Assert.AreEqual(456, capturedRequest.RequestPropertyB, "Overrides value of mapped property");
        Assert.AreEqual("overridden", capturedRequest.RequestPropertyC, "Overrides value of unmapped property");
    }

    #endregion

    #region InvokeAsync<TResponse>(InteractionDispatcher<TRequest>, CancellationToken)

    [TestMethod]
    public async Task InvokeAsync_TResponse_Throws_WhenDispatchArgumentIsNull()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
            ViewModelPropertyC = "ignored",
        };

        var builder = ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => builder.InvokeAsync<FakeResponse>(null!, CancellationToken.None));
    }

    [TestMethod]
    public async Task InvokeAsync_TResponse_ReturnsNull_WhenHasValidationResults()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var validationResults = new ValidationResult[] {
            new("Example error", [nameof(FakeRequest.RequestPropertyA)]),
        };
        var fakeException = new InvalidRequestException(Guid.Empty, validationResults);

        var response = await ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel)
            .InvokeAsync<FakeResponse>((_, _) => throw fakeException, CancellationToken.None);

        Assert.IsNull(response);
    }

    [TestMethod]
    public async Task InvokeAsync_TResponse_ReturnsExpectedResponse()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var fakeResponse = new FakeResponse();

        var response = await ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel)
            .InvokeAsync<FakeResponse>((_, _) => InteractionTask.FromResult(fakeResponse), CancellationToken.None);

        Assert.AreSame(fakeResponse, response);
    }

    #endregion

    #region InvokeAsync(InteractionDispatcher<TRequest>, CancellationToken)

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenDispatchArgumentIsNull()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
            ViewModelPropertyC = "ignored",
        };

        var builder = ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => builder.InvokeAsync(null!, CancellationToken.None));
    }

    [TestMethod]
    public async Task InvokeAsync_ProvidesExpectedCancellationToken()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        using var cancellationTokenSource = new CancellationTokenSource();

        CancellationToken? capturedCancellationToken = null;
        InteractionTask capturer(FakeRequest _, CancellationToken cancellationToken)
        {
            capturedCancellationToken = cancellationToken;
            return InteractionTask.FromResult(new FakeResponse());
        }

        await ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel)
            .InvokeAsync(capturer, cancellationTokenSource.Token);

        Assert.AreEqual(cancellationTokenSource.Token, capturedCancellationToken);
    }

    [TestMethod]
    public async Task InvokeAsync_ThrowsOtherExceptions()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var fakeException = new UnexpectedException();

        var builder = ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel);

        var exception = await Assert.ThrowsExactlyAsync<UnexpectedException>(()
            => builder.InvokeAsync((_, _) => throw fakeException, CancellationToken.None));

        Assert.AreSame(fakeException, exception);
    }

    [TestMethod]
    public async Task InvokeAsync_ThrowsInvalidRequestException_WhenNoValidationResults()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var fakeException = new InvalidRequestException();

        var builder = ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel);

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => builder.InvokeAsync((_, _) => throw fakeException, CancellationToken.None));

        Assert.AreSame(fakeException, exception);
    }

    [TestMethod]
    public async Task InvokeAsync_MapsErrorsToViewModel_WhenHasValidationResults()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var validationResults = new ValidationResult[] {
            new("Example error 1", [nameof(FakeRequest.RequestPropertyA)]),
            new("Example error 2", [nameof(FakeRequest.RequestPropertyB)]),
        };
        var fakeException = new InvalidRequestException(Guid.Empty, validationResults);

        await ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel)
            .InvokeAsync((_, _) => throw fakeException, CancellationToken.None);

        var modelState = mockController.Object.ModelState;
        Assert.IsFalse(modelState.IsValid);
        Assert.AreEqual(2, modelState.ErrorCount);
        Assert.AreEqual("Example error 1", modelState.First(x => x.Key == nameof(FakeViewModel.ViewModelPropertyA)).Value!.Errors[0].ErrorMessage);
        Assert.AreEqual("Example error 2", modelState.First(x => x.Key == nameof(FakeViewModel.ViewModelPropertyB)).Value!.Errors[0].ErrorMessage);
    }

    [TestMethod]
    public async Task InvokeAsync_HandlesUnmappedProperties_WhenHasValidationResults()
    {
        var mockController = new Mock<Controller>();

        var fakeViewModel = new FakeViewModel {
            ViewModelPropertyA = "abc",
            ViewModelPropertyB = "123",
        };

        var validationResults = new ValidationResult[] {
            new("Example error", ["UnmappedProperty"]),
        };
        var fakeException = new InvalidRequestException(Guid.Empty, validationResults);

        await ControllerInteractionExtensions.MapInteractionRequest<FakeRequest>(mockController.Object, fakeViewModel)
            .InvokeAsync((_, _) => throw fakeException, CancellationToken.None);

        var modelState = mockController.Object.ModelState;
        Assert.IsFalse(modelState.IsValid);
        Assert.AreEqual(1, modelState.ErrorCount);
        Assert.AreEqual("Example error", modelState.First(x => x.Key == "").Value!.Errors[0].ErrorMessage);
    }

    #endregion
}
