using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <exclude/>
public static class SwaggerExtensions
{
    private static string GetXmlFileName(Type type)
    {
        return type.Assembly.Location
            .Replace(".dll", ".xml")
            .Replace(".exe", ".xml");
    }

    /// <summary>
    /// Setup swagger for endpoints.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupSwagger(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(config => {
            config.UseInlineDefinitionsForEnums();

            // Include XML comments for 'SignIn.Core.Models.dll' assembly.
            config.IncludeXmlComments(GetXmlFileName(typeof(OrganisationModel)));
            // Include XML comments for 'SignIn.Core.PublicModels.dll' assembly.
            config.IncludeXmlComments(GetXmlFileName(typeof(OrganisationFilter)));
            // Include XML comments for 'SignIn.Core.PublicApi.dll' assembly.
            config.IncludeXmlComments(GetXmlFileName(typeof(Program)));
        });
    }
}
