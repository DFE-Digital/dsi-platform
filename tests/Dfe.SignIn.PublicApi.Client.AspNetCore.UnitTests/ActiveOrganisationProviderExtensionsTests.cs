using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore.UnitTests;

[TestClass]
public sealed class ActiveOrganisationProviderExtensionsTests
{
    #region GetActiveOrganisationAsync(this IActiveOrganisationProvider, HttpContext)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task GetActiveOrganisationAsync_Throws_WhenProviderArgumentIsNull()
    {
        var mockContext = new Mock<HttpContext>();

        await ActiveOrganisationProviderExtensions.GetActiveOrganisationAsync(null!, mockContext.Object);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task GetActiveOrganisationAsync_Throws_WhenContextArgumentIsNull()
    {
        var mockProvider = new Mock<IActiveOrganisationProvider>();

        await ActiveOrganisationProviderExtensions.GetActiveOrganisationAsync(mockProvider.Object, null!);
    }

    [TestMethod]
    public async Task GetActiveOrganisationAsync_Throws_WhenActiveOrganisationStateIsMissing()
    {
        var mockContext = new Mock<HttpContext>();
        var mockProvider = new Mock<IActiveOrganisationProvider>();

        mockProvider
            .Setup(mock => mock.GetActiveOrganisationStateAsync(It.IsAny<IHttpContext>()))
            .ReturnsAsync(null as ActiveOrganisationState);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => ActiveOrganisationProviderExtensions.GetActiveOrganisationAsync(mockProvider.Object, mockContext.Object)
        );
        Assert.AreEqual("Missing active organisation state.", exception.Message);
    }

    [TestMethod]
    public async Task GetActiveOrganisationAsync_ReturnsActiveOrganisation_WhenNull()
    {
        var mockContext = new Mock<HttpContext>();
        var mockProvider = new Mock<IActiveOrganisationProvider>();

        mockProvider
            .Setup(mock => mock.GetActiveOrganisationStateAsync(It.IsAny<IHttpContext>()))
            .ReturnsAsync(new ActiveOrganisationState {
                Organisation = null,
            });

        var result = await ActiveOrganisationProviderExtensions.GetActiveOrganisationAsync(mockProvider.Object, mockContext.Object);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetActiveOrganisationAsync_ReturnsActiveOrganisation()
    {
        var mockContext = new Mock<HttpContext>();
        var mockProvider = new Mock<IActiveOrganisationProvider>();

        var fakeOrganisation = new OrganisationDetails {
            Id = new Guid("2d69d768-7284-4342-a86e-c5b0c3380523"),
            Name = "Example Organisation",
        };

        mockProvider
            .Setup(mock => mock.GetActiveOrganisationStateAsync(It.IsAny<IHttpContext>()))
            .ReturnsAsync(new ActiveOrganisationState {
                Organisation = fakeOrganisation,
            });

        var result = await ActiveOrganisationProviderExtensions.GetActiveOrganisationAsync(mockProvider.Object, mockContext.Object);

        Assert.IsNotNull(result);
        Assert.AreSame(fakeOrganisation, result);
    }

    #endregion
}
