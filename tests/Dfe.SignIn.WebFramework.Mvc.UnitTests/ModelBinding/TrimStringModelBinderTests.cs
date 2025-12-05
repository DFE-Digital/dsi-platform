using System.Globalization;
using Dfe.SignIn.WebFramework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.ModelBinding;

[TestClass]
public sealed class TrimStringModelBinderTests
{
    [TestMethod]
    public async Task DecoratesModelBinderWithTrimStringBehaviour()
    {
        var mockModelBinder = new Mock<IModelBinder>();

        var trimStringModelBinder = new TrimStringModelBinder(mockModelBinder.Object);

        var mockValueProvider = new Mock<IValueProvider>();
        mockValueProvider
            .Setup(x => x.GetValue(It.Is<string>(k => k == "TestKey")))
            .Returns(new ValueProviderResult(new StringValues(" abc "), CultureInfo.CurrentCulture));

        var bindingContext = new DefaultModelBindingContext {
            ValueProvider = mockValueProvider.Object,
        };

        await trimStringModelBinder.BindModelAsync(bindingContext);

        var value = bindingContext.ValueProvider.GetValue("TestKey");
        Assert.HasCount(1, value);
        Assert.AreEqual("abc", value.First());
    }
}
