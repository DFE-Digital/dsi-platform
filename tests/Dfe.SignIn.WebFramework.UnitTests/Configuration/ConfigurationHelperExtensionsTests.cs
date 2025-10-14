using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Dfe.SignIn.WebFramework.UnitTests.Configuration;

[TestClass]
public sealed class ConfigurationHelperExtensionsTests
{
    private sealed class FakeJsonObject
    {
        public string[] Values { get; set; } = [];
        public int Foo { get; set; }
    }

    #region GetJson<T>(this IConfigurationSection, string)

    [TestMethod]
    public void GetJson_Throws_WhenSectionArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ConfigurationHelperExtensions.GetJson<FakeJsonObject>(null!, "ExampleKey"));
    }

    [TestMethod]
    public void GetJson_Throws_WhenKeyArgumentIsNull()
    {
        var sectionMock = new Mock<IConfigurationSection>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ConfigurationHelperExtensions.GetJson<FakeJsonObject>(sectionMock.Object, null!));
    }

    [TestMethod]
    public void GetJson_Throws_WhenKeyArgumentIsEmptyString()
    {
        var sectionMock = new Mock<IConfigurationSection>();

        Assert.ThrowsExactly<ArgumentException>(()
            => ConfigurationHelperExtensions.GetJson<FakeJsonObject>(sectionMock.Object, ""));
    }

    [TestMethod]
    public void GetJson_ReturnsJsonEncodedData()
    {
        var section = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ExampleKey"] = /*lang=json,strict*/ """
                {
                    "Values": [ "one", "two" ],
                    "Foo": 42
                }
                """,
            })
            .Build();

        var result = ConfigurationHelperExtensions.GetJson<FakeJsonObject>(section, "ExampleKey");

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(new string[] { "one", "two" }, result.Values);
        Assert.AreEqual(42, result.Foo);
    }

    [TestMethod]
    public void GetJson_ReturnsNull_WhenKeyNotPresent()
    {
        var section = new ConfigurationBuilder().Build();

        var result = ConfigurationHelperExtensions.GetJson<FakeJsonObject>(section, "ExampleKey");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetJson_ReturnsValue_WhenNotJson()
    {
        var section = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ExampleKey:0"] = "one",
                ["ExampleKey:1"] = "two",
            })
            .Build();

        var result = ConfigurationHelperExtensions.GetJson<string[]>(section, "ExampleKey");

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(new string[] { "one", "two" }, result);
    }

    #endregion

    #region GetJsonList<T>(this IConfigurationSection, string)

    [TestMethod]
    public void GetJsonList_Throws_WhenSectionArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ConfigurationHelperExtensions.GetJsonList<FakeJsonObject>(null!, "ExampleKey"));
    }

    [TestMethod]
    public void GetJsonList_Throws_WhenKeyArgumentIsNull()
    {
        var sectionMock = new Mock<IConfigurationSection>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ConfigurationHelperExtensions.GetJsonList<FakeJsonObject>(sectionMock.Object, null!));
    }

    [TestMethod]
    public void GetJsonList_Throws_WhenKeyArgumentIsEmptyString()
    {
        var sectionMock = new Mock<IConfigurationSection>();

        Assert.ThrowsExactly<ArgumentException>(()
            => ConfigurationHelperExtensions.GetJsonList<FakeJsonObject>(sectionMock.Object, ""));
    }

    [TestMethod]
    public void GetJsonList_ReturnsJsonEncodedData()
    {
        var section = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ExampleKey"] = /*lang=json,strict*/ """
                [
                    {
                        "Values": [ "one", "two" ],
                        "Foo": 42
                    }
                ]
                """,
            })
            .Build();

        var result = ConfigurationHelperExtensions.GetJsonList<FakeJsonObject>(section, "ExampleKey");

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(new string[] { "one", "two" }, result[0].Values);
        Assert.AreEqual(42, result[0].Foo);
    }

    [TestMethod]
    public void GetJsonList_ReturnsNull_WhenKeyNotPresent()
    {
        var section = new ConfigurationBuilder().Build();

        var result = ConfigurationHelperExtensions.GetJsonList<FakeJsonObject>(section, "ExampleKey");

        Assert.IsNotNull(result);
        Assert.HasCount(0, result);
    }

    #endregion

    #region GetJsonList(this IConfigurationSection, string)

    [TestMethod]
    public void GetJsonList_string_Throws_WhenSectionArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ConfigurationHelperExtensions.GetJsonList(null!, "ExampleKey"));
    }

    [TestMethod]
    public void GetJsonList_string_Throws_WhenKeyArgumentIsNull()
    {
        var sectionMock = new Mock<IConfigurationSection>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ConfigurationHelperExtensions.GetJsonList(sectionMock.Object, null!));
    }

    [TestMethod]
    public void GetJsonList_string_Throws_WhenKeyArgumentIsEmptyString()
    {
        var sectionMock = new Mock<IConfigurationSection>();

        Assert.ThrowsExactly<ArgumentException>(()
            => ConfigurationHelperExtensions.GetJsonList(sectionMock.Object, ""));
    }

    [TestMethod]
    public void GetJsonList_string_ReturnsJsonEncodedData()
    {
        var section = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ExampleKey"] = /*lang=json,strict*/ """
                [
                    "one",
                    "two"
                ]
                """,
            })
            .Build();

        var result = ConfigurationHelperExtensions.GetJsonList(section, "ExampleKey");

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(new string[] { "one", "two" }, result);
    }

    [TestMethod]
    public void GetJsonList_string_ReturnsNull_WhenKeyNotPresent()
    {
        var section = new ConfigurationBuilder().Build();

        var result = ConfigurationHelperExtensions.GetJsonList(section, "ExampleKey");

        Assert.IsNotNull(result);
        Assert.HasCount(0, result);
    }

    #endregion
}
