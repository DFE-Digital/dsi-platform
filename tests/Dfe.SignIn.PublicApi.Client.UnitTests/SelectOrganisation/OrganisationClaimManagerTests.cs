using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.Internal;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class OrganisationClaimManagerTests
{
    private const string FakeUserId = "6c2afda0-62ce-4f88-9a23-a252bc647856";
    private const string FakeSessionId = "92c37f9d-253b-42cd-95eb-ddeb9d1e03b8";

    private const string FakeOrganisation1ClaimValue = /*lang=json,strict*/ """
        { "id": "6ab8e6c9-f953-42e4-be0d-4a7ace4b229d" }
    """;

    private const string FakeOrganisation2ClaimValue = /*lang=json,strict*/ """
        { "id": "7d6f1125-dd53-4d61-8ae6-3e20d5091b1c" }
    """;

    private const string FakeOrganisation3ClaimValue = /*lang=json,strict*/ """
        { "id": "7122a5eb-a66c-49ca-a3cc-73c123680bbd" }
    """;

    private static void SetupFakeOptions(
        AutoMocker autoMocker,
        AuthenticationOrganisationSelectorOptions? options = null)
    {
        autoMocker.GetMock<IOptions<AuthenticationOrganisationSelectorOptions>>()
            .Setup(mock => mock.Value)
            .Returns(options ?? new AuthenticationOrganisationSelectorOptions());
    }

    private static Mock<IHttpContext> SetupFakeContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        return mockContext;
    }

    private static ClaimsPrincipal SetupFakeUser(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(DsiClaimTypes.UserId, FakeUserId),
            new Claim(DsiClaimTypes.SessionId, FakeSessionId),
        ]));

        mockContext.Setup(x => x.User)
            .Returns(fakeUser);

        return fakeUser;
    }

    private static void SetupFakeGetUserAccessToServiceResponse(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IInteractor<GetUserAccessToServiceRequest, GetUserAccessToServiceResponse>>()
            .Setup(mock => mock.InvokeAsync(
                It.IsAny<GetUserAccessToServiceRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new GetUserAccessToServiceResponse {
                Roles = [
                    new Role {
                        Id = new Guid("e06ce7a4-f846-4ccc-830a-0a10aed627cf"),
                        Name = "Example Role 1",
                        Code = "EX123",
                        NumericId = "123",
                        Status = new() {
                            Id = 1,
                        },
                    },
                    new Role {
                        Id = new Guid("93a0525a-354c-4dc6-944f-220251271360"),
                        Name = "Example Role 2",
                        Code = "EX456",
                        NumericId = "456",
                        Status = new() {
                            Id = 1,
                        },
                    },
                    new Role {
                        Id = new Guid("c2e23dc4-d4a7-4538-9993-1101c648ab65"),
                        Name = "Example Role 3",
                        Code = "EX890",
                        NumericId = "890",
                        Status = new() {
                            Id = 0,
                        },
                    },
                ],
            });
    }

    #region UpdateOrganisationClaim(HttpContext, string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task UpdateOrganisationClaim_Throws_WhenContextArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        await manager.UpdateOrganisationClaimAsync(null!, "null");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task UpdateOrganisationClaim_Throws_WhenOrganisationJsonArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, null!);
    }

    [TestMethod]
    public async Task UpdateOrganisationClaim_RetainsPrimaryIdentity()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, FakeOrganisation1ClaimValue);

        var primaryIdentity = capturedPrincipal?.GetPrimaryIdentity();
        Assert.IsNotNull(primaryIdentity);
        Assert.IsTrue(
            primaryIdentity.HasClaim(DsiClaimTypes.SessionId, FakeSessionId)
        );
        Assert.IsTrue(
            primaryIdentity.HasClaim(DsiClaimTypes.UserId, FakeUserId)
        );
    }

    [TestMethod]
    public async Task UpdateOrganisationClaim_AddsSessionIdClaim()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, FakeOrganisation1ClaimValue);

        var dsiIdentity = capturedPrincipal?.GetDsiIdentity();
        Assert.IsNotNull(dsiIdentity);
        Assert.IsTrue(
            dsiIdentity.HasClaim(DsiClaimTypes.SessionId, FakeSessionId)
        );
    }

    [TestMethod]
    public async Task UpdateOrganisationClaim_AddsOrganisationClaim()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, FakeOrganisation1ClaimValue);

        var dsiIdentity = capturedPrincipal?.GetDsiIdentity();
        Assert.IsNotNull(dsiIdentity);
        Assert.IsTrue(
            dsiIdentity.HasClaim(DsiClaimTypes.Organisation, FakeOrganisation1ClaimValue)
        );
    }

    [TestMethod]
    public async Task UpdateOrganisationClaim_InvokesUpdateClaimsIdentityDelegate()
    {
        var autoMocker = new AutoMocker();

        ClaimsIdentity? updateClaimsIdentityInvokedWith = null;
        SetupFakeOptions(autoMocker, new AuthenticationOrganisationSelectorOptions {
            UpdateClaimsIdentity = (identity) => {
                updateClaimsIdentityInvokedWith = identity;
                return Task.FromResult(identity);
            },
        });

        var mockContext = SetupFakeContext(autoMocker);
        var fakeUser = SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, FakeOrganisation1ClaimValue);

        Assert.IsNotNull(updateClaimsIdentityInvokedWith);
        Assert.AreEqual(PublicApiConstants.AuthenticationType, updateClaimsIdentityInvokedWith.AuthenticationType);
    }

    [TestMethod]
    public async Task UpdateOrganisationClaim_ReplacesAnyExistingOrganisationClaims()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        var fakeUser = SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        fakeUser.AddIdentity(new ClaimsIdentity([
            new Claim(DsiClaimTypes.Organisation, FakeOrganisation1ClaimValue, JsonClaimValueTypes.Json),
            new Claim(DsiClaimTypes.Organisation, FakeOrganisation2ClaimValue, JsonClaimValueTypes.Json),
        ], PublicApiConstants.AuthenticationType));

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, FakeOrganisation3ClaimValue);

        Assert.IsNotNull(capturedPrincipal);
        var dfeSignInIdentities = capturedPrincipal.Identities
            .Where(identity => identity.AuthenticationType == PublicApiConstants.AuthenticationType);
        Assert.AreEqual(1, dfeSignInIdentities.Count());
        Assert.AreEqual(1, capturedPrincipal.FindAll(DsiClaimTypes.Organisation).Count());
        Assert.AreEqual(FakeOrganisation3ClaimValue, capturedPrincipal.FindFirst(DsiClaimTypes.Organisation)?.Value);
    }

    [TestMethod]
    public async Task UpdateOrganisationClaim_DoesNotFetchRolesFromPublicApi_WhenNoFlagsSpecified()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, FakeOrganisation1ClaimValue);

        autoMocker.Verify<IInteractor<GetUserAccessToServiceRequest, GetUserAccessToServiceResponse>>(
            interactor => interactor.InvokeAsync(
                It.IsAny<GetUserAccessToServiceRequest>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never);
    }

    [DataRow(FetchRoleClaimsFlag.RoleId, DsiClaimTypes.RoleId, "e06ce7a4-f846-4ccc-830a-0a10aed627cf", "93a0525a-354c-4dc6-944f-220251271360")]
    [DataRow(FetchRoleClaimsFlag.RoleName, DsiClaimTypes.RoleName, "Example Role 1", "Example Role 2")]
    [DataRow(FetchRoleClaimsFlag.RoleCode, DsiClaimTypes.RoleCode, "EX123", "EX456")]
    [DataRow(FetchRoleClaimsFlag.RoleNumericId, DsiClaimTypes.RoleNumericId, "123", "456")]
    [DataTestMethod]
    public async Task UpdateOrganisationClaim_AddsRoleIdClaims_WhenFlagSpecified(
        FetchRoleClaimsFlag flag, string claimType, string expectedValue1, string expectedValue2)
    {
        var autoMocker = new AutoMocker();
        SetupFakeGetUserAccessToServiceResponse(autoMocker);

        SetupFakeOptions(autoMocker, new() {
            FetchRoleClaimsFlags = flag,
        });

        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, FakeOrganisation1ClaimValue);

        var dsiIdentity = capturedPrincipal?.GetDsiIdentity();
        Assert.IsNotNull(dsiIdentity);
        Assert.IsTrue(
            dsiIdentity.HasClaim(DsiClaimTypes.Organisation, FakeOrganisation1ClaimValue)
        );
        Assert.IsTrue(
            dsiIdentity.HasClaim(claimType, expectedValue1)
        );
        Assert.IsTrue(
            dsiIdentity.HasClaim(claimType, expectedValue2)
        );
    }

    #endregion
}
