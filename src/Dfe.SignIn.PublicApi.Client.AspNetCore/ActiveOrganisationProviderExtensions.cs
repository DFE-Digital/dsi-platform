using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore;

/// <summary>
/// Extensions methods for <see cref="IActiveOrganisationProvider"/> services.
/// </summary>
public static class ActiveOrganisationProviderExtensions
{
    /// <summary>
    /// Gets the active organisation of a user.
    /// </summary>
    /// <param name="provider">The <see cref="IActiveOrganisationProvider"/> instance.</param>
    /// <param name="context">The HTTP context.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="provider"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="context"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If organisation state is missing.</para>
    /// </exception>
    public static async Task<OrganisationDetails?> GetActiveOrganisationAsync(this IActiveOrganisationProvider provider, HttpContext context)
    {
        ExceptionHelpers.ThrowIfArgumentNull(provider, nameof(provider));
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        var adaptedContext = new HttpContextAspNetCoreAdapter(context);
        var state = await provider.GetActiveOrganisationStateAsync(adaptedContext)
            ?? throw new InvalidOperationException("Missing active organisation state.");
        return state.Organisation;
    }
}
