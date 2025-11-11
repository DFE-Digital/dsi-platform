namespace Dfe.SignIn.WebFramework.UnitTests;

[TestClass]
public sealed class MapToAttributeTests
{
    public sealed record FakeRequest
    {
        public string? ExampleProperty { get; init; }
    }

    [TestMethod]
    public void Constructor_Throws_WhenRequestPropertyNameArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => new MapToAttribute<FakeRequest>(null!));
    }

    [TestMethod]
    public void Constructor_Throws_WhenRequestPropertyNameArgumentIsEmpty()
    {
        Assert.ThrowsExactly<ArgumentException>(()
            => new MapToAttribute<FakeRequest>(""));
    }

    [TestMethod]
    public void Constructor_SetsExpectedProperties()
    {
        var attribute = new MapToAttribute<FakeRequest>(
            requestPropertyName: nameof(FakeRequest.ExampleProperty)
        );

        Assert.AreEqual(typeof(FakeRequest), attribute.RequestType);
        Assert.AreEqual(nameof(FakeRequest.ExampleProperty), attribute.RequestPropertyName);
    }

    [TestMethod]
    [DataRow(RequestMappingOptions.None, false, false)]
    [DataRow(RequestMappingOptions.Everything, true, true)]
    [DataRow(RequestMappingOptions.Value, true, false)]
    [DataRow(RequestMappingOptions.ValidationErrors, false, true)]
    public void Constructor_SetsExpectedFlags(RequestMappingOptions flags, bool valueFlag, bool validationErrorsFlag)
    {
        var attribute = new MapToAttribute<FakeRequest>(
            requestPropertyName: nameof(FakeRequest.ExampleProperty), flags);

        Assert.AreEqual(valueFlag, attribute.Flags.HasFlag(RequestMappingOptions.Value));
        Assert.AreEqual(validationErrorsFlag, attribute.Flags.HasFlag(RequestMappingOptions.ValidationErrors));
    }
}
