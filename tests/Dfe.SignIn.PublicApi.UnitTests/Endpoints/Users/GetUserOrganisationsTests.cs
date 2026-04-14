using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Users;

[TestClass]
public sealed class GetUserOrganisationsTests
{
    private static readonly Guid FakeUserId = new("a1b2c3d4-0000-0000-0000-000000000001");

    private static readonly Organisation FakeOrganisation = new() {
        Id = new Guid("a1b2c3d4-0000-0000-0000-000000000010"),
        Name = "Test Organisation",
        Status = OrganisationStatus.Open,
    };

    [TestMethod]
    public async Task Returns200_WithOrganisations_WhenUserHasVisibleOrgs()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetUserOrganisationsRequest>(
            new GetUserOrganisationsResponse {
                Organisations = [FakeOrganisation],
            }
        );

        var result = await UserEndpoints.GetUserOrganisations(
            FakeUserId,
            autoMocker.Get<IInteractionDispatcher>()
        );

        var ok = result.Result as Ok<IEnumerable<Organisation>>;
        Assert.IsNotNull(ok);
        Assert.HasCount(1, ok.Value!.ToArray());
        Assert.AreEqual(FakeOrganisation.Id, ok.Value!.First().Id);
    }

    [TestMethod]
    public async Task Returns404_WhenUserNotFoundException()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<GetUserOrganisationsRequest>(
            UserNotFoundException.FromUserId(FakeUserId)
        );

        var result = await UserEndpoints.GetUserOrganisations(
            FakeUserId,
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsInstanceOfType<NotFound>(result.Result);
    }
}
