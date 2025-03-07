namespace Dfe.SignIn.PublicApi.Configuration;

/// <exclude/>
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
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddAutoMapper(options => {
            // Add mapping profiles here...
            //options.AddProfile<ExampleMappingProfile>();
        });
    }
}
