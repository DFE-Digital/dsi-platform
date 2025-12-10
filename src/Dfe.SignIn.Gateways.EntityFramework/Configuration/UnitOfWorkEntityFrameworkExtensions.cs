
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration;

/// <summary>
/// Extension methods for setting up the Entity Framework Unit Of Work.
/// </summary>
public static class UnitOfWorkEntityFrameworkExtensions
{
    /// <summary>
    /// Adds the required configuration for using Entity Framework Core, unit of work implementation.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="section">Configuration section.</param>
    /// <param name="addDirectoriesUnitOfWork">Register Directories unit of work.</param>
    /// <param name="addOrganisationsUnitOfWork">Register Organisations unit of work.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="section"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddUnitOfWorkEntityFrameworkServices(this IServiceCollection services, IConfiguration section, bool addDirectoriesUnitOfWork, bool addOrganisationsUnitOfWork)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(section, nameof(section));

        if (addDirectoriesUnitOfWork || addOrganisationsUnitOfWork) {
            services.AddScoped<IEntityFrameworkTransactionContext, EntityFrameworkTransactionContext>();
            services.Decorate<IInteractionDispatcher, ProtectTransactionInteractionDispatcher>();
        }

        AddUnitOfWork<DbDirectoriesContext, DirectoriesUnitOfWork, IUnitOfWorkDirectories>(
            services,
            section,
            "Directories:ConnectionString",
            addDirectoriesUnitOfWork);

        AddUnitOfWork<DbOrganisationsContext, OrganisationsUnitOfWork, IUnitOfWorkOrganisations>(
            services,
            section,
            "Organisations:ConnectionString",
            addOrganisationsUnitOfWork);

        return services;
    }

    private static void AddUnitOfWork<TContext, TUnitOfWork, TUnitOfWorkInterface>(
        IServiceCollection services,
        IConfiguration section,
        string configKey,
        bool register)
        where TContext : DbContext
        where TUnitOfWork : EntityFrameworkUnitOfWork, TUnitOfWorkInterface
        where TUnitOfWorkInterface : class, IUnitOfWork
    {
        if (!register) {
            return;
        }

        var connectionString = section
            .GetRequiredSection(configKey)
            .Value ?? throw new InvalidOperationException($"Missing connection string {configKey}.");

        services.AddDbContext<TContext>(options => {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<TUnitOfWorkInterface, TUnitOfWork>();
    }
}
