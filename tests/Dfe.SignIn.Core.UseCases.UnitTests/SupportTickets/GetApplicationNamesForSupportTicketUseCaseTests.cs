using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.UseCases.SupportTickets;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.SupportTickets;

[TestClass]
public sealed class GetApplicationNamesForSupportTicketUseCaseTests
{

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.NewGuid(),
            Name = "Service No Params",
            ServiceParams = [],
            ClientId = string.Empty,
            ClientSecret = string.Empty
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.NewGuid(),
            Name = "Another Service No Params",
            ServiceParams = [],
            ClientId = string.Empty,
            ClientSecret = string.Empty
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.NewGuid(),
            Name = "Service Other Param",
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            ServiceParams = [
                new ServiceParamEntity {
                    ParamName = "somethingElse",
                    ParamValue = "true"
                }
            ]
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.NewGuid(),
            Name = "Service Hidden",
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            ServiceParams = [
                new ServiceParamEntity {
                    ParamName = "helpHidden",
                    ParamValue = "true"
                }
            ]
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.NewGuid(),
            Name = "Service Visible",
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            ServiceParams =
            [
                new ServiceParamEntity {
                    ParamName = "helpHidden",
                    ParamValue = "false"
                }
            ]
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.NewGuid(),
            Name = "Service Mixed Params",
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            ServiceParams =
            [
                new ServiceParamEntity {
                    ParamName = "foo",
                    ParamValue = "bar"
                },
                new ServiceParamEntity
                {
                    ParamName = "helpHidden",
                    ParamValue = "true"
                }
            ]
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.NewGuid(),
            Name = "Child Service",
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            IsChildService = true,
            ServiceParams = []
        });

        await ctx.SaveChangesAsync();
    }

    [TestMethod]
    public async Task Services_WithNoParams_AreReturned()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationNamesForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationNamesForSupportTicketRequest { }
        );

        CollectionAssert.Contains(response.Applications.Select(x => x.Name).ToArray(), "Service No Params");
    }

    [TestMethod]
    public async Task Services_WithUnrelatedParams_AreReturned()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationNamesForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationNamesForSupportTicketRequest { }
        );

        CollectionAssert.Contains(response.Applications.Select(x => x.Name).ToArray(), "Service Other Param");
    }

    [TestMethod]
    public async Task Services_WithHelpHiddenTrue_AreExcluded()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationNamesForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationNamesForSupportTicketRequest { }
        );

        CollectionAssert.DoesNotContain(response.Applications.Select(x => x.Name).ToArray(), "Service Hidden");
    }

    [TestMethod]
    public async Task Services_WithHelpHiddenFalse_AreReturned()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationNamesForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationNamesForSupportTicketRequest { }
        );

        CollectionAssert.Contains(response.Applications.Select(x => x.Name).ToArray(), "Service Visible");
    }

    [TestMethod]
    public async Task Services_WithMixedParams_IncludingHelpHiddenTrue_AreExcluded()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationNamesForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationNamesForSupportTicketRequest { }
        );

        CollectionAssert.DoesNotContain(response.Applications.Select(x => x.Name).ToArray(), "Service Mixed Params");
    }

    [TestMethod]
    public async Task ChildServices_AreExcluded()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationNamesForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationNamesForSupportTicketRequest { }
        );

        CollectionAssert.DoesNotContain(response.Applications.Select(x => x.Name).ToArray(), "Child Service");
    }

    [TestMethod]
    public async Task OnlyExpectedServices_AreReturned()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationNamesForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationNamesForSupportTicketRequest { }
        );

        CollectionAssert.AreEquivalent(
            new[]
            {
                "Service No Params",
                "Another Service No Params",
                "Service Other Param",
                "Service Visible",
                "Other (please specify)",
                "None"
            },
            response.Applications.Select(x => x.Name).ToArray());
    }

    [TestMethod]
    public async Task OnlyExpectedServices_AreReturned_Ordered()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationNamesForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationNamesForSupportTicketRequest { }
        );

        var services = response.Applications.ToArray();

        Assert.AreEqual("Another Service No Params", services[0].Name);
        Assert.AreEqual("Service No Params", services[1].Name);
        Assert.AreEqual("Service Other Param", services[2].Name);
        Assert.AreEqual("Service Visible", services[3].Name);
        Assert.AreEqual("Other (please specify)", services[4].Name);
        Assert.AreEqual("None", services[5].Name);
    }
}
