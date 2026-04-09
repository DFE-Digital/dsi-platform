using System.Text.Json;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.NodeApi.Client.Access;
using Dfe.SignIn.NodeApi.Client.Access.Models;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Access;

[TestClass]
public sealed class GetUserServiceAccessNodeRequesterTests
{
    private static readonly Guid UserId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001");
    private static readonly Guid ServiceId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000002");
    private static readonly Guid OrganisationId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000003");
    private static readonly Guid RoleId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000004");

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserServiceAccessRequest,
            GetUserServiceAccessNodeRequester
        >();
    }

    private static GetUserServiceAccessNodeRequester CreateRequester(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        var client = new HttpClient(new FakeHttpMessageHandler(handler)) {
            BaseAddress = new Uri("http://access.localhost/")
        };
        return new GetUserServiceAccessNodeRequester(client);
    }

    [TestMethod]
    public async Task ReturnsNullAccess_WhenApiReturns404()
    {
        var requester = CreateRequester((_, _)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)));

        var response = await requester.InvokeAsync(
            new GetUserServiceAccessRequest {
                UserId = UserId,
                ServiceId = ServiceId,
                OrganisationId = OrganisationId,
            }
        );

        Assert.IsNull(response.Access);
    }

    [TestMethod]
    public async Task CallsCorrectUrl()
    {
        HttpRequestMessage? captured = null;
        var requester = CreateRequester((req, _) => {
            captured = req;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        });

        await requester.InvokeAsync(
            new GetUserServiceAccessRequest {
                UserId = UserId,
                ServiceId = ServiceId,
                OrganisationId = OrganisationId,
            }
        );

        Assert.IsNotNull(captured);
        Assert.AreEqual(
            $"http://access.localhost/users/{UserId}/services/{ServiceId}/organisations/{OrganisationId}",
            captured.RequestUri!.ToString()
        );
    }

    [TestMethod]
    public async Task ReturnsExpectedAccess_WhenApiReturnsData()
    {
        var dto = new UserServiceAccessDto {
            UserId = UserId,
            ServiceId = ServiceId,
            OrganisationId = OrganisationId,
            Roles = [
                new UserServiceRoleDto {
                    Id = RoleId,
                    Name = "Test Role",
                    Code = "test-role",
                    NumericId = 100L,
                }
            ],
            Identifiers = [
                new UserServiceIdentifierDto {
                    Key = "saUserId",
                    Value = "SA-12345",
                }
            ],
        };

        var requester = CreateRequester((_, _) => Task.FromResult(
            new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new StringContent(
                    JsonSerializer.Serialize(dto),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            }
        ));

        var response = await requester.InvokeAsync(
            new GetUserServiceAccessRequest {
                UserId = UserId,
                ServiceId = ServiceId,
                OrganisationId = OrganisationId,
            }
        );

        Assert.IsNotNull(response.Access);
        Assert.AreEqual(UserId, response.Access.UserId);
        Assert.AreEqual(ServiceId, response.Access.ServiceId);
        Assert.AreEqual(OrganisationId, response.Access.OrganisationId);

        var roles = response.Access.Roles.ToArray();
        Assert.HasCount(1, roles);
        Assert.AreEqual(RoleId, roles[0].Id);
        Assert.AreEqual("test-role", roles[0].Code);
        Assert.AreEqual(100L, roles[0].NumericId);

        var identifiers = response.Access.Identifiers.ToArray();
        Assert.HasCount(1, identifiers);
        Assert.AreEqual("saUserId", identifiers[0].Key);
        Assert.AreEqual("SA-12345", identifiers[0].Value);
    }
}
