using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
///  Represents a service that can resolve an implementation for building the navigation menu items.
/// </summary>
public interface IServiceNavigationBuilder
{
    Task<NavigationItemViewModel[]> Build(ClaimsPrincipal user);
}

/// <summary>
/// A builder that constructs the Navigation Menu Items
/// </summary>
public sealed class ServiceNavigationBuilder : IServiceNavigationBuilder
{
    private readonly PlatformOptions platformOptions;
    private readonly IUsersApiClient userApiClient;

    public ServiceNavigationBuilder(
        IOptions<PlatformOptions> platformOptionsAccessor, IUsersApiClient userApiClient)
    {
        this.platformOptions = platformOptionsAccessor.Value;
        this.userApiClient = userApiClient;
    }

    public async Task<NavigationItemViewModel[]> Build(
        ClaimsPrincipal user)
    {
        if (!user.Identity?.IsAuthenticated == true) {
            return [];
        }

        var items = new List<NavigationItemViewModel>
        {
            new StandardNavigationItemViewModel()
            {
                Href = new Uri(this.platformOptions.ServicesUrl, "my-services"),
                Text = "Services",
            },
            new StandardNavigationItemViewModel()
            {
                Href = new Uri(this.platformOptions.ServicesUrl, "organisations"),
                Text = "Organisations",
            }
        };

        if (user.HasApproverClaim()) {

            var pendingapproverCount = await this.userApiClient.PendingApprovalCount();

            items.Add(new StandardNavigationItemViewModel {
                Href = new Uri(this.platformOptions.ServicesUrl, "approvals/users"),
                Text = "Manage users",
            });

            items.Add(new CountNavigationItemViewModel {
                Href = new Uri(this.platformOptions.ServicesUrl, "access-requests"),
                Text = "Requests",
                Count = pendingapproverCount.Count
            });
        }

        items.Add(new StandardNavigationItemViewModel {
            Href = this.platformOptions.ProfileUrl,
            Text = "Profile",
            IsActive = true,
        });

        items.Add(new StandardNavigationItemViewModel {
            Href = this.platformOptions.HelpUrl,
            Text = "Help",
        });

        return [.. items];
    }
}
