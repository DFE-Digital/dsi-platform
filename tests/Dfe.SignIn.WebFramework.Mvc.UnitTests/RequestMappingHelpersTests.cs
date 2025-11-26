namespace Dfe.SignIn.WebFramework.Mvc.UnitTests;

[TestClass]
public sealed class RequestMappingHelpersTests
{
    public sealed class FakeViewModel
    {
        [MapTo<FakeRequest>(nameof(FakeRequest.RequestPropertyA))]
        [MapTo<FakeRequestWithMissingProperty>(nameof(FakeRequestWithMissingProperty.RequestPropertyA))]
        public int ViewModelPropertyA { get; set; }

        [MapTo<FakeRequest>(nameof(FakeRequest.RequestPropertyB), RequestMappingOptions.ValidationErrors)]
        [MapTo<FakeRequestWithMissingProperty>("MissingProperty", RequestMappingOptions.ValidationErrors)]
        public string? ViewModelPropertyB { get; set; }

        public string? ViewModelPropertyC { get; set; }

#pragma warning disable IDE0051, S1144 // Remove unused private members
        private string? ViewModelPropertyD { get; set; }
#pragma warning restore IDE0051, S1144 // Remove unused private members

        public static string? StaticViewModelProperty { get; set; }
    }

    public sealed record FakeRequest
    {
        public required int RequestPropertyA { get; set; }
        public required int RequestPropertyB { get; set; }
    }

    public sealed record FakeRequestWithMissingProperty
    {
        public required int RequestPropertyA { get; set; }
    }

    #region GetMappings<TRequest>(Type)

    [TestMethod]
    public void GetMappings_Throws_WhenViewModelTypeArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => RequestMappingHelpers.GetMappings<FakeRequest>(null!));
    }

    [TestMethod]
    public void GetMappings_ReturnsExpectedMappings()
    {
        var mappings = RequestMappingHelpers.GetMappings<FakeRequest>(typeof(FakeViewModel));

        var expectedProperties = new string[] {
            nameof(FakeRequest.RequestPropertyA),
            nameof(FakeRequest.RequestPropertyB),
        };
        CollectionAssert.AreEquivalent(expectedProperties, mappings.Keys.ToArray());

        var values = mappings.Values.ToArray();
        Assert.HasCount(2, values);

        Assert.AreEqual(RequestMappingOptions.Everything, values[0].Flags);
        Assert.AreEqual(
            typeof(FakeViewModel).GetProperty(nameof(FakeViewModel.ViewModelPropertyA)),
            values[0].ViewModelProperty
        );
        Assert.AreEqual(
            typeof(FakeRequest).GetProperty(nameof(FakeRequest.RequestPropertyA)),
            values[0].RequestProperty
        );

        Assert.AreEqual(RequestMappingOptions.ValidationErrors, values[1].Flags);
        Assert.AreEqual(
            typeof(FakeViewModel).GetProperty(nameof(FakeViewModel.ViewModelPropertyB)),
            values[1].ViewModelProperty
        );
        Assert.AreEqual(
            typeof(FakeRequest).GetProperty(nameof(FakeRequest.RequestPropertyB)),
            values[1].RequestProperty
        );
    }

    [TestMethod]
    public void GetMappings_OmitsMissingProperties()
    {
        var mappings = RequestMappingHelpers.GetMappings<FakeRequestWithMissingProperty>(typeof(FakeViewModel));

        var expectedProperties = new string[] {
            nameof(FakeRequestWithMissingProperty.RequestPropertyA),
        };
        CollectionAssert.AreEquivalent(expectedProperties, mappings.Keys.ToArray());

        var values = mappings.Values.ToArray();
        Assert.HasCount(1, values);

        Assert.AreEqual(RequestMappingOptions.Everything, values[0].Flags);
        Assert.AreEqual(
            typeof(FakeViewModel).GetProperty(nameof(FakeViewModel.ViewModelPropertyA)),
            values[0].ViewModelProperty
        );
        Assert.AreEqual(
            typeof(FakeRequestWithMissingProperty).GetProperty(nameof(FakeRequestWithMissingProperty.RequestPropertyA)),
            values[0].RequestProperty
        );
    }

    [TestMethod]
    public void GetMappings_ReturnsCachedResponse()
    {
        var response1 = RequestMappingHelpers.GetMappings<FakeRequest>(typeof(FakeViewModel));
        var response2 = RequestMappingHelpers.GetMappings<FakeRequest>(typeof(FakeViewModel));

        Assert.AreSame(response1, response2);
    }

    #endregion
}
