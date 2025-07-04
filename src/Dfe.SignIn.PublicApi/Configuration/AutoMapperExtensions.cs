using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.MappingProfiles;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Extension methods for setting up AutoMapper.
/// </summary>
public static class AutoMapperExtensions
{
    /// <summary>
    /// Setup AutoMapper and specify any custom mapping profiles.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupAutoMapper(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddAutoMapper(options => {
            options.AddProfile<SelectOrganisationMappingProfile>();
        });
        services.AddAutoMapper(options => {
            options.AddProfile<OrganisationDetailsMappingProfile>();
        });
    }
}
