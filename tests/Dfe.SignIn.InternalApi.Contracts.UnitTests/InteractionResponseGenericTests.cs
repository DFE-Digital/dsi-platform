using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfe.SignIn.InternalApi.Contracts.UnitTests;

[TestClass]
public sealed class InteractionResponseGenericTests
{
    #region FromResponse(TResponse)

    [TestMethod]
    public void FromResponse_Throws_WhenResponseArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionResponse<PingResponse>.FromResponse(null!));
    }

    [TestMethod]
    public void FromResponse_ReturnsExpectedInteractionResponse()
    {
        var fakeResponse = new PingResponse { Value = 123 };

        var interactionResponse = InteractionResponse<PingResponse>.FromResponse(fakeResponse);

        Assert.IsTrue(interactionResponse.Success);
        Assert.IsNotNull(interactionResponse.Content);
        Assert.AreEqual("Dfe.SignIn.Core.Contracts.Diagnostics.PingResponse", interactionResponse.Content.Type);
        Assert.AreSame(fakeResponse, interactionResponse.Content.Data);
    }

    #endregion

    #region FromException(Exception, IExceptionJsonSerializer)

    [TestMethod]
    public void FromException_Throws_WhenExceptionArgumentIsNull()
    {
        var mockExceptionSerializer = new Mock<IExceptionJsonSerializer>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionResponse<PingResponse>.FromException(
                exception: null!,
                exceptionSerializer: mockExceptionSerializer.Object
            ));
    }

    [TestMethod]
    public void FromException_Throws_WhenExceptionSerializerArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionResponse<PingResponse>.FromException(
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

        var interactionResponse = InteractionResponse<PingResponse>.FromException(
            fakeException, exceptionSerializer);

        Assert.IsFalse(interactionResponse.Success);
        Assert.IsNotNull(interactionResponse.Exception);

        var exceptionJson = interactionResponse.Exception.Value;
        Assert.AreEqual("Dfe.SignIn.Base.Framework.InvalidRequestException", exceptionJson.GetProperty("type").GetString());
        Assert.AreEqual(fakeException.Message, exceptionJson.GetProperty("message").GetString());
        Assert.AreEqual(fakeException.InvocationId, exceptionJson.GetProperty("invocationId").GetGuid());

        var validationResultJson = exceptionJson.GetProperty("validationResults").EnumerateArray().First();
        Assert.AreEqual(fakeException.ValidationResults.First().ErrorMessage, validationResultJson.GetProperty("errorMessage").GetString());
        var memberNameJson = validationResultJson.GetProperty("memberNames").EnumerateArray().First();
        Assert.AreEqual(fakeException.ValidationResults.First().MemberNames.First(), memberNameJson.GetString());
    }

    #endregion
}
