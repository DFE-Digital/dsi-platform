using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfe.SignIn.InternalApi.Contracts.UnitTests;

[TestClass]
public sealed class FailedInteractionResponseTests
{
    #region FromException(Exception, IExceptionJsonSerializer)

    [TestMethod]
    public void FromException_Throws_WhenExceptionArgumentIsNull()
    {
        var mockExceptionSerializer = new Mock<IExceptionJsonSerializer>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => FailedInteractionResponse.FromException(
                exception: null!,
                exceptionSerializer: mockExceptionSerializer.Object
            ));
    }

    [TestMethod]
    public void FromException_Throws_WhenExceptionSerializerArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => FailedInteractionResponse.FromException(
                exception: new Exception(),
                exceptionSerializer: null!
            ));
    }

    [TestMethod]
    public void FromException_ReturnsExpectedInteractionResponse()
    {
        var serviceProvider = new ServiceCollection()
            .ConfigureDfeSignInJsonSerializerOptions()
            .BuildServiceProvider();

        var exceptionSerializer = serviceProvider.GetRequiredService<IExceptionJsonSerializer>();

        var fakeException = new InvalidRequestException(
            invocationId: new Guid("dd731f58-2263-48c7-bac8-9b139fe7e5dd"),
            validationResults: [
                new("Enter a valid email address.", ["EmailAddress"]),
            ]
        );

        var interactionResponse = FailedInteractionResponse.FromException(
            fakeException, exceptionSerializer);

        var exceptionJson = interactionResponse.Exception;
        Assert.AreEqual("Dfe.SignIn.Base.Framework.InvalidRequestException", exceptionJson.GetProperty("type").GetString());
        Assert.AreEqual(fakeException.Message, exceptionJson.GetProperty("message").GetString());
        Assert.AreEqual(fakeException.InvocationId, exceptionJson.GetProperty("invocationId").GetGuid());

        var validationResultJson = exceptionJson.GetProperty("validationResults").EnumerateArray().First();
        Assert.AreEqual("Enter a valid email address.", validationResultJson.GetProperty("errorMessage").GetString());
        var memberNameJson = validationResultJson.GetProperty("memberNames").EnumerateArray().First();
        Assert.AreEqual("emailAddress", memberNameJson.GetString());
    }

    #endregion
}
