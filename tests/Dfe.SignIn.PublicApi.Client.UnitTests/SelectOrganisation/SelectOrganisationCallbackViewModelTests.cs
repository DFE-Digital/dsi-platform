using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackViewModelTests
{
    #region FromRequest(HttpRequest)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public Task FromRequest_Throws_WhenRequestArgumentIsNull()
    {
        return SelectOrganisationCallbackViewModel.FromRequest(null!);
    }

    [TestMethod]
    public async Task ViewModelFromRequest_PopulatesViewModelFromRequest()
    {
        var mockRequest = new Mock<IHttpRequest>();
        mockRequest.Setup(mock => mock.ReadFormAsync())
            .ReturnsAsync(new Dictionary<string, StringValues> {
                { "payloadType", PayloadTypeConstants.Id },
                { "payload", "{data}" },
                { "sig", "{sig}" },
                { "kid", "6bb65413-12db-41b9-a606-82103e6d5c0c" },
            });

        var viewModel = await SelectOrganisationCallbackViewModel.FromRequest(mockRequest.Object);

        var expectedViewModel = new SelectOrganisationCallbackViewModel {
            PayloadType = PayloadTypeConstants.Id,
            Payload = "{data}",
            Sig = "{sig}",
            Kid = "6bb65413-12db-41b9-a606-82103e6d5c0c",
        };
        Assert.AreEqual(expectedViewModel, viewModel);
    }

    #endregion
}
