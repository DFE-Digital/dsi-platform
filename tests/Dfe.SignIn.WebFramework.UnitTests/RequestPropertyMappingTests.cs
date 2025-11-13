namespace Dfe.SignIn.WebFramework.UnitTests;

[TestClass]
public sealed class RequestPropertyMappingTests
{
    private sealed class FakeViewModel
    {
#pragma warning disable S3459
        public int ViewModelProperty { get; set; }
#pragma warning restore S3459
    }

    private sealed record FakeRequest
    {
#pragma warning disable S3459
        public int RequestProperty { get; set; }
#pragma warning restore S3459
    }

    [TestMethod]
    public void Constructor_AssignsExpectedProperties()
    {
        var viewModelProperty = typeof(FakeViewModel).GetProperty(nameof(FakeViewModel.ViewModelProperty))!;
        var requestProperty = typeof(FakeRequest).GetProperty(nameof(FakeRequest.RequestProperty))!;

        var mapping = new RequestPropertyMapping(
            viewModelProperty, requestProperty, RequestMappingOptions.ValidationErrors);

        Assert.AreSame(viewModelProperty, mapping.ViewModelProperty);
        Assert.AreSame(requestProperty, mapping.RequestProperty);
        Assert.AreEqual(RequestMappingOptions.ValidationErrors, mapping.Flags);
    }
}
