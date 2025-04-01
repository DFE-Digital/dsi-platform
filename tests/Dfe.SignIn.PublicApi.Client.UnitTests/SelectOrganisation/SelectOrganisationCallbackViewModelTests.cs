using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackViewModelTests
{
    #region FromRequest(HttpRequest)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void FromRequest_Throws_WhenRequestArgumentIsNull()
    {
        SelectOrganisationCallbackViewModel.FromRequest(null!);
    }

    [TestMethod]
    public void ViewModelFromRequest_PopulatesViewModelFromRequest()
    {
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(mock => mock.Form)
            .Returns(new FormCollection(new() {
                { "payloadType", PayloadTypeConstants.Id },
                { "payload", "{data}" },
                { "sig", "{sig}" },
                { "kid", "6bb65413-12db-41b9-a606-82103e6d5c0c" },
            }));

        var viewModel = SelectOrganisationCallbackViewModel.FromRequest(mockRequest.Object);

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
