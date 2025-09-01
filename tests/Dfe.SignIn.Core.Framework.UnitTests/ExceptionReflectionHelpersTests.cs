using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class ExceptionReflectionHelpersTests
{
    private class FakeReflectionHelperException : InteractionException
    {
        [Persist]
        public Type PropertyWithType { get; set; } = typeof(FakeReflectionHelperException);

        [Persist]
        public Type? NullablePropertyWithType { get; set; }

        [Persist]
        public Assembly PropertyWithAssembly { get; set; } = typeof(FakeReflectionHelperException).Assembly;

        [Persist]
        public Assembly? NullablePropertyWithAssembly { get; set; } = typeof(FakeReflectionHelperException).Assembly;

        [Persist]
        [SuppressMessage("roslyn", "CA1822",
            Justification = "Property needs to be instance member for unit test."
        )]
        public string PropertyWithoutGetter { set { } }

        [Persist]
        public string PropertyWithoutSetter { get; } = "";

        [Persist]
        public string SomeProperty { get; private set; } = "";
    }

    private class FakeDerivedReflectionHelperException : FakeReflectionHelperException
    {
        [Persist]
        public string AnotherProperty { get; private set; } = "";
    }

    private class FakeOtherReflectionHelperException : Exception
    {
        public string SomeProperty { get; private set; } = "";
    }

    #region GetExceptionTypeByFullName(string)

    [TestMethod]
    public void GetExceptionTypeByFullName_Throws_WhenFullNameArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ExceptionReflectionHelpers.GetExceptionTypeByFullName(null!));
    }

    [TestMethod]
    public void GetExceptionTypeByFullName_Throws_WhenFullNameArgumentIsEmptyString()
    {
        Assert.ThrowsExactly<ArgumentException>(()
            => ExceptionReflectionHelpers.GetExceptionTypeByFullName(""));
    }

    [DataRow("System.Exception", typeof(Exception))]
    [DataRow("Dfe.SignIn.Core.Framework.InteractionException", typeof(InteractionException))]
    [DataRow("Dfe.SignIn.Core.Framework.UnexpectedException", typeof(UnexpectedException))]
    [DataTestMethod]
    public void GetExceptionTypeByFullName_ReturnsExpectedType(string fullName, Type expectedType)
    {
        var exceptionType = ExceptionReflectionHelpers.GetExceptionTypeByFullName(fullName);

        Assert.AreEqual(expectedType, exceptionType);
    }

    #endregion

    #region IsSerializableProperty(PropertyInfo)

    [TestMethod]
    public void IsSerializableProperty_ReturnsFalse_WhenPropertyIsNotDeclaredWithinAnInteractionException()
    {
        var property = typeof(FakeOtherReflectionHelperException).GetProperty(
            nameof(FakeOtherReflectionHelperException.SomeProperty)
        );

        bool result = ExceptionReflectionHelpers.IsSerializableProperty(property!);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSerializableProperty_Throws_WhenPropertyArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ExceptionReflectionHelpers.IsSerializableProperty(null!));
    }

    [TestMethod]
    public void IsSerializableProperty_ReturnsFalse_WhenPropertyTypeIsType()
    {
        var property = typeof(FakeReflectionHelperException).GetProperty(
            nameof(FakeReflectionHelperException.PropertyWithType)
        );

        bool result = ExceptionReflectionHelpers.IsSerializableProperty(property!);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSerializableProperty_ReturnsFalse_WhenNullablePropertyTypeIsType()
    {
        var property = typeof(FakeReflectionHelperException).GetProperty(
            nameof(FakeReflectionHelperException.NullablePropertyWithType)
        );

        bool result = ExceptionReflectionHelpers.IsSerializableProperty(property!);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSerializableProperty_ReturnsFalse_WhenPropertyTypeIsFromReflectionNamespace()
    {
        var property = typeof(FakeReflectionHelperException).GetProperty(
            nameof(FakeReflectionHelperException.PropertyWithAssembly)
        );

        bool result = ExceptionReflectionHelpers.IsSerializableProperty(property!);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSerializableProperty_ReturnsFalse_WhenNullablePropertyTypeIsFromReflectionNamespace()
    {
        var property = typeof(FakeReflectionHelperException).GetProperty(
            nameof(FakeReflectionHelperException.NullablePropertyWithAssembly)
        );

        bool result = ExceptionReflectionHelpers.IsSerializableProperty(property!);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSerializableProperty_ReturnsFalse_WhenPropertyDoesNotHaveGetter()
    {
        var property = typeof(FakeReflectionHelperException).GetProperty(
            nameof(FakeReflectionHelperException.PropertyWithoutGetter)
        );

        bool result = ExceptionReflectionHelpers.IsSerializableProperty(property!);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSerializableProperty_ReturnsFalse_WhenPropertyDoesNotHaveSetter()
    {
        var property = typeof(FakeReflectionHelperException).GetProperty(
            nameof(FakeReflectionHelperException.PropertyWithoutSetter)
        );

        bool result = ExceptionReflectionHelpers.IsSerializableProperty(property!);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSerializableProperty_ReturnsTrue_WhenPropertyIsDeclaredInInteractionException()
    {
        var property = typeof(FakeReflectionHelperException).GetProperty(
            nameof(FakeReflectionHelperException.SomeProperty)
        );

        bool result = ExceptionReflectionHelpers.IsSerializableProperty(property!);

        Assert.IsTrue(result);
    }

    #endregion

    #region GetSerializableExceptionProperties(Type)

    [TestMethod]
    public void GetSerializableExceptionProperties_Throws_WhenExceptionTypeArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ExceptionReflectionHelpers.GetSerializableExceptionProperties(null!));
    }

    [TestMethod]
    public void GetSerializableExceptionProperties_ReturnsExpectedProperties()
    {
        var properties1 = ExceptionReflectionHelpers.GetSerializableExceptionProperties(
            typeof(FakeDerivedReflectionHelperException)
        );

        Assert.AreEqual(2, properties1.Count());
        Assert.IsTrue(properties1.Any(property =>
            property.DeclaringType == typeof(FakeReflectionHelperException) &&
            property.Name == nameof(FakeReflectionHelperException.SomeProperty)
        ));
        Assert.IsTrue(properties1.Any(property =>
            property.DeclaringType == typeof(FakeDerivedReflectionHelperException) &&
            property.Name == nameof(FakeDerivedReflectionHelperException.AnotherProperty)
        ));

        var properties2 = ExceptionReflectionHelpers.GetSerializableExceptionProperties(
            typeof(FakeOtherReflectionHelperException)
        );

        Assert.AreEqual(0, properties2.Count());
    }

    #endregion
}
