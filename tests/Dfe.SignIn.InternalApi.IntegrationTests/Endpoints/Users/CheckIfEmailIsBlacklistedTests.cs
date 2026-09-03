
using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class CheckIfEmailIsBlacklistedTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpointUrl => "/internal/email/check";

    public CheckIfEmailIsBlacklistedTests(InternalApiWebApplicationFactory webAppFactory)
        : base(webAppFactory)
    {
    }

    [Fact]
    public async Task ReturnsFalseWhenEmailIsEmpty()
    {
        var authenticatedClient = this.CreateClient()
            .WithAuthentication();

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointUrl, new CheckIsBlockedEmailAddressRequest {
            EmailAddress = string.Empty
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("alex.hunter@example.com")]
    [InlineData("xadmin@example.com")]
    [InlineData("staffmember@example.com")]
    [InlineData("patrick@example.com")]
    [InlineData("peter@example.com")]
    public async Task ReturnsExpectedResponse_WhenEmailAddressIsPermitted(string emailAddress)
    {
        var authenticatedClient = this.CreateClient()
            .WithAuthentication();

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointUrl, new CheckIsBlockedEmailAddressRequest {
            EmailAddress = emailAddress
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CheckIsBlockedEmailAddressResponse>();
        Assert.NotNull(body);
        Assert.False(body.IsBlocked);
    }

    [Theory]
    [InlineData("admin@example.com")]
    [InlineData("admin123@example.com")]
    [InlineData("admin.test@example.com")]
    [InlineData("admin_test@example.com")]
    [InlineData("ADMIN@example.com")]
    [InlineData("staff@example.com")]
    [InlineData("STAFF@example.com")]
    [InlineData("pat@example.com")]
    [InlineData("pat123@example.com")]
    [InlineData("pat.test@example.com")]
    [InlineData("alex.hunter@blocked.com")]
    [InlineData("alex.hunter@BLOCKED.COM")]
    [InlineData("alex.hunter@blocked.other.com")]
    [InlineData("admin@blocked.com")]
    public async Task ReturnsExpectedResponse_WhenEmailAddressIsBlocked(string emailAddress)
    {
        var authenticatedClient = this.CreateClient()
            .WithAuthentication();

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointUrl, new CheckIsBlockedEmailAddressRequest {
            EmailAddress = emailAddress
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CheckIsBlockedEmailAddressResponse>();
        Assert.NotNull(body);
        Assert.True(body.IsBlocked);
    }
}
