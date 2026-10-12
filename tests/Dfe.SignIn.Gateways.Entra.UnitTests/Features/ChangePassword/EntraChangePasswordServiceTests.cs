using System.Net;
using System.Text.Json;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Gateways.Entra.ChangePassword;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.SignIn.Gateways.Entra.UnitTests.Features.ChangePassword;

[TestClass]
public sealed class EntraChangePasswordServiceTests
{
    private static EntraChangePasswordService CreateService()
    {
        return new EntraChangePasswordService(NullLogger<EntraChangePasswordService>.Instance);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsInvalidCurrentPassword_WhenCurrentPasswordIsEmpty()
    {
        var service = CreateService();
        var result = await service.ChangePasswordAsync("", "newPassword", new GraphAccessToken { Token = "token", ExpiresOn = DateTimeOffset.UtcNow });

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(EntraPasswordErrors.InvalidCurrentPasswordCode, result.Error.Code);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsPolicyViolation_WhenNewPasswordIsEmpty()
    {
        var service = CreateService();
        var result = await service.ChangePasswordAsync("oldPassword", "", new GraphAccessToken { Token = "token", ExpiresOn = DateTimeOffset.UtcNow });

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(EntraPasswordErrors.PasswordPolicyViolationCode, result.Error.Code);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsUnexpected_WhenTokenMissing()
    {
        var service = CreateService();
        var result = await service.ChangePasswordAsync("oldPassword", "newPassword", new GraphAccessToken { Token = "", ExpiresOn = DateTimeOffset.UtcNow });

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Entra.Password.Unexpected", result.Error.Code);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsSuccess_WhenGraphRequestSucceeds()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;
        using var httpClient = new HttpClient(new TestHttpMessageHandler(async request => {
            capturedRequest = request;
            capturedBody = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }));
        var service = CreateService(httpClient);

        var result = await service.ChangePasswordAsync("oldPassword", "newPassword", CreateToken());

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(HttpMethod.Post, capturedRequest.Method);
        Assert.AreEqual("/v1.0/me/changePassword", capturedRequest.RequestUri!.AbsolutePath);
        using var body = JsonDocument.Parse(capturedBody!);
        Assert.AreEqual("oldPassword", body.RootElement.GetProperty("currentPassword").GetString());
        Assert.AreEqual("newPassword", body.RootElement.GetProperty("newPassword").GetString());
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsInvalidCurrentPassword_WhenGraphIdentifiesOldPassword()
    {
        using var httpClient = CreateGraphErrorClient("The old password is incorrect. paramName: oldPassword");
        var service = CreateService(httpClient);

        var result = await service.ChangePasswordAsync("oldPassword", "newPassword", CreateToken());

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(EntraPasswordErrors.InvalidCurrentPasswordCode, result.Error.Code);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsPolicyViolation_WhenGraphIdentifiesNewPassword()
    {
        using var httpClient = CreateGraphErrorClient("The new password is too weak. paramName: newPassword");
        var service = CreateService(httpClient);

        var result = await service.ChangePasswordAsync("oldPassword", "newPassword", CreateToken());

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(EntraPasswordErrors.PasswordPolicyViolationCode, result.Error.Code);
        Assert.AreEqual("The new password is too weak.", result.Error.Description);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsUnexpected_WhenGraphErrorDoesNotIdentifyPasswordParameter()
    {
        using var httpClient = CreateGraphErrorClient("Access was denied.");
        var service = CreateService(httpClient);

        var result = await service.ChangePasswordAsync("oldPassword", "newPassword", CreateToken());

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Entra.Password.Unexpected", result.Error.Code);
        Assert.AreEqual("Access was denied.", result.Error.Description);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsUnexpected_WhenGraphRequestThrows()
    {
        using var httpClient = new HttpClient(new TestHttpMessageHandler(_ => throw new InvalidOperationException("Graph unavailable.")));
        var service = CreateService(httpClient);

        var result = await service.ChangePasswordAsync("oldPassword", "newPassword", CreateToken());

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Entra.Password.Unexpected", result.Error.Code);
        Assert.AreEqual("Graph unavailable.", result.Error.Description);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsUnexpected_WhenAccessTokenIsNull()
    {
        var service = CreateService();

        var result = await service.ChangePasswordAsync("oldPassword", "newPassword", null!);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Entra.Password.Unexpected", result.Error.Code);
    }

    private static EntraChangePasswordService CreateService(HttpClient httpClient)
        => new(NullLogger<EntraChangePasswordService>.Instance, httpClient);

    private static GraphAccessToken CreateToken()
        => new() { Token = "fake-token", ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) };

    private static HttpClient CreateGraphErrorClient(string message)
    {
        var error = JsonSerializer.Serialize(new {
            error = new { code = "Request_BadRequest", message }
        });
        return new HttpClient(new TestHttpMessageHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) {
            Content = new StringContent(error, System.Text.Encoding.UTF8, "application/json")
        })));
    }

    private sealed class TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request);
    }
}
