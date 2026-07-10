using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Dfe.SignIn.PublicApi.Models;
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
        Category = OrganisationCategory.Government,
        CategoryId = "011"
    };

    private static readonly Organisation FakeOrganisationWithLocalAuthority = new() {
        Id = new Guid("f3b2e3d4-0000-0000-0000-000000000011"),
        Name = "Test Organisation 2",
        Status = OrganisationStatus.Open,
        Category = OrganisationCategory.Government,
        CategoryId = "011",
        LocalAuthority = new Core.Contracts.Organisations.LocalAuthority(
            new Guid("f1b1e1d1-1120-0000-0000-000000000011"),
            "Test LA",
            "Rx1"),
        PhaseOfEducation = 2

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

        var ok = result.Result as Ok<IEnumerable<UserOrganisationDto>>;
        Assert.IsNotNull(ok);
        Assert.HasCount(1, ok.Value!.ToArray());
        Assert.AreEqual(FakeOrganisation.Id, ok.Value!.First().Id);

        Assert.IsNotNull(ok.Value!.First().PhaseOfEducation);
        Assert.AreEqual("Not applicable", ok.Value!.First().PhaseOfEducation.Name);
        Assert.AreEqual("0", ok.Value!.First().PhaseOfEducation.Id);
    }

    [TestMethod]
    public async Task Returns200_WithOrganisations_WhenUserHasVisibleIncludesLocalAuthority()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetUserOrganisationsRequest>(
            new GetUserOrganisationsResponse {
                Organisations = [FakeOrganisationWithLocalAuthority],
            }
        );

        var result = await UserEndpoints.GetUserOrganisations(
            FakeUserId,
            autoMocker.Get<IInteractionDispatcher>()
        );

        var ok = result.Result as Ok<IEnumerable<UserOrganisationDto>>;
        Assert.IsNotNull(ok);
        Assert.HasCount(1, ok.Value!.ToArray());
        Assert.AreEqual(FakeOrganisationWithLocalAuthority.Id, ok.Value!.First().Id);
        Assert.AreEqual(FakeOrganisationWithLocalAuthority.LocalAuthority.Id, ok.Value!.First().LocalAuthority.Id);
        Assert.AreEqual(FakeOrganisationWithLocalAuthority.LocalAuthority.Code, ok.Value!.First().LocalAuthority.Code);
        Assert.AreEqual(FakeOrganisationWithLocalAuthority.LocalAuthority.Name, ok.Value!.First().LocalAuthority.Name);

        Assert.IsNotNull(ok.Value!.First().PhaseOfEducation);
        Assert.AreEqual("Local Authority", ok.Value!.First().PhaseOfEducation.Name);
        Assert.AreEqual("002", ok.Value!.First().PhaseOfEducation.Id);
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
