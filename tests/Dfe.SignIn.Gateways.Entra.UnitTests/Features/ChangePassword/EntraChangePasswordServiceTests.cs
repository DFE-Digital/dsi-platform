using Dfe.SignIn.Base.Framework.Results;
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
}
