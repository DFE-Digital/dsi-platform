using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Notifications;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class InitiateChangeEmailAddressUseCaseTests
{
    private static readonly Guid UserIdMatchingEmail = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691");
    private static readonly Guid UserIdMatchingEntraOid = Guid.Parse("74f539fa-5de2-4b15-b983-24ca9612b7cb");
    private static readonly Guid UserEntraOid = Guid.Parse("ecf31b2d-03e8-4f00-9035-32d2dd4a9ed3");

    private static async Task<DbDirectoriesContext> SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryDirectoriesDb();

        ctx.Users.Add(new UserEntity {
            Sub = UserIdMatchingEmail,
            IsEntra = false,
            FirstName = "Alex",
            LastName = "Johnson",
            Email = "alex.johnson@example.com",
            Password = "",
            Salt = "",
            Status = 1
        });

        ctx.Users.Add(new UserEntity {
            Sub = UserIdMatchingEntraOid,
            IsEntra = true,
            FirstName = "Bob",
            LastName = "Simons",
            Email = "bob.simons@example.com",
            Password = "",
            Salt = "",
            EntraOid = UserEntraOid,
            Status = 0
        });

        await ctx.SaveChangesAsync();

        return ctx;
    }

    [TestMethod]
    public async Task ThrowsUserNotFoundException()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<InitiateChangeEmailAddressUseCase>();

        await Assert.ThrowsExactlyAsync<UserNotFoundException>(()
         => interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
             ClientId = "test",
             IsSelfInvoked = true,
             NewEmailAddress = "abcs.johnson@example.com",
             UserId = Guid.NewGuid()
         }));
    }

    [TestMethod]
    public async Task GeneratesErrorWhenUserKeepsSameEmail()
    {
        var autoMocker = new AutoMocker();
        var db = await SetupFakeDatabaseAsync(autoMocker);

        autoMocker.MockResponse<GetUserStatusRequest>(
            new GetUserStatusResponse {
                AccountStatus = AccountStatus.Active,
                UserExists = true,
                UserId = UserIdMatchingEmail
            }
        );

        var interactor = autoMocker.CreateInstance<InitiateChangeEmailAddressUseCase>();

        await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
         => interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
             ClientId = "test",
             IsSelfInvoked = true,
             NewEmailAddress = "alex.johnson@example.com",
             UserId = UserIdMatchingEmail
         }));
    }

    [TestMethod]
    public async Task GeneratesNewCodeWhenNoCodeExists()
    {
        var autoMocker = new AutoMocker();

        autoMocker.Use(Options.Create(
        new PlatformOptions {
            HelpUrl = new Uri("https://help.test"),
            ProfileUrl = new Uri("https://profile.test")
        }));

        var db = await SetupFakeDatabaseAsync(autoMocker);

        autoMocker.MockResponse<GetUserStatusRequest>(
            new GetUserStatusResponse {
                UserExists = false
            }
        );

        var interactor = autoMocker.CreateInstance<InitiateChangeEmailAddressUseCase>();

        var response = await interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
            ClientId = "test",
            IsSelfInvoked = true,
            NewEmailAddress = "alex.johnson+11@example.com",
            UserId = UserIdMatchingEmail
        });

        var userCode = db.UserCodes.SingleOrDefault(x => x.Uid == UserIdMatchingEmail && x.CodeType == "changeemail");
        Assert.IsNotNull(userCode);
        Assert.AreEqual("alex.johnson+11@example.com", userCode.Email);
        Assert.IsFalse(string.IsNullOrWhiteSpace(userCode.Code));
        Assert.AreEqual(8, userCode.Code.Length);
    }

    [TestMethod]
    public async Task GeneratesNewCodeWhenCodeExists()
    {
        var autoMocker = new AutoMocker();

        autoMocker.Use(Options.Create(
        new PlatformOptions {
            HelpUrl = new Uri("https://help.test"),
            ProfileUrl = new Uri("https://profile.test")
        }));

        var db = await SetupFakeDatabaseAsync(autoMocker);

        db.UserCodes.Add(new UserCodeEntity {
            Uid = UserIdMatchingEmail,
            CodeType = "changeemail",
            Email = "alex.johnson@example.com",
            Code = "OLDCODE1",
            ClientId = "old-client",
            RedirectUri = "old-uri",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        autoMocker.MockResponse<GetUserStatusRequest>(
            new GetUserStatusResponse {
                UserExists = false
            }
        );

        var interactor = autoMocker.CreateInstance<InitiateChangeEmailAddressUseCase>();

        var response = await interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
            ClientId = "test",
            IsSelfInvoked = true,
            NewEmailAddress = "alex.johnson+11@example.com",
            UserId = UserIdMatchingEmail
        });

        var userCode = db.UserCodes.SingleOrDefault(x => x.Uid == UserIdMatchingEmail && x.CodeType == "changeemail");

        Assert.AreEqual("alex.johnson+11@example.com", userCode.Email);
        Assert.AreNotEqual("OLDCODE1", userCode.Code);
        Assert.IsNotNull(userCode.Code);
        Assert.AreEqual(8, userCode.Code.Length);
    }

    [TestMethod]
    public async Task EmailNewEmailAddressTheGeneratedCodeTest()
    {
        var autoMocker = new AutoMocker();

        autoMocker.Use(Options.Create(
        new PlatformOptions {
            HelpUrl = new Uri("https://help.test"),
            ProfileUrl = new Uri("https://profile.test")
        }));

        var db = await SetupFakeDatabaseAsync(autoMocker);

        db.UserCodes.Add(new UserCodeEntity {
            Uid = UserIdMatchingEmail,
            CodeType = "changeemail",
            Email = "alex.johnson@example.com",
            Code = "OLDCODE1",
            ClientId = "old-client",
            RedirectUri = "old-uri",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        autoMocker.MockResponse<GetUserStatusRequest>(
            new GetUserStatusResponse {
                UserExists = false
            }
        );

        var interactor = autoMocker.CreateInstance<InitiateChangeEmailAddressUseCase>();

        var interactionMock = autoMocker.GetMock<IInteractionDispatcher>();

        var sentEmails = new List<SendEmailNotificationRequest>();

        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<InteractionContext<SendEmailNotificationRequest>>()))
            .Callback<InteractionContext<SendEmailNotificationRequest>>(ctx => {
                sentEmails.Add(ctx.Request);
            });

        _ = await interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
            ClientId = "test",
            IsSelfInvoked = true,
            NewEmailAddress = "alex.johnson+11@example.com",
            UserId = UserIdMatchingEmail
        });

        Assert.AreEqual(2, sentEmails.Count);

        var verificationEmail = sentEmails.Single(x =>
    x.RecipientEmailAddress == "alex.johnson+11@example.com");

        Assert.AreEqual(
            "8a6b7625-87d5-41bc-bc58-035343571d81",
            verificationEmail.TemplateId);

        Assert.AreEqual(
            "Alex",
            verificationEmail.Personalisation["firstName"]);

        Assert.AreEqual(
            "Johnson",
            verificationEmail.Personalisation["lastName"]);

        Assert.AreEqual(
            "alex.johnson+11@example.com",
            verificationEmail.Personalisation["email"]);

        Assert.IsTrue(
    verificationEmail.Personalisation.ContainsKey("code"));

        Assert.AreEqual(
            8,
            ((string)verificationEmail.Personalisation["code"]).Length);
    }
}
