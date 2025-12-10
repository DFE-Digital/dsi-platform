
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    public static IServiceCollection AddUnitOfWorkEntityFrameworkServices(
        this IServiceCollection services,
        IConfiguration section,
        bool addDirectoriesUnitOfWork,
        bool addOrganisationsUnitOfWork)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(section, nameof(section));

        if (addDirectoriesUnitOfWork || addOrganisationsUnitOfWork) {
            services.TryAddSingleton(TimeProvider.System);
            services.AddScoped<IEntityFrameworkTransactionContext, EntityFrameworkTransactionContext>();
            services.Decorate<IInteractionDispatcher, ProtectTransactionInteractionDispatcher>();
            services.AddScoped<TimestampInterceptor>();
        }

        AddUnitOfWork<IUnitOfWorkDirectories, UnitOfWorkDirectories, DbDirectoriesContext>(
            services,
            section,
            "Directories:ConnectionString",
            addDirectoriesUnitOfWork);

        AddUnitOfWork<IUnitOfWorkOrganisations, UnitOfWorkOrganisations, DbOrganisationsContext>(
            services,
            section,
            "Organisations:ConnectionString",
            addOrganisationsUnitOfWork);

        return services;
    }

    private static void AddUnitOfWork<TUnitOfWorkContract, TUnitOfWorkConcrete, TDbContext>(
        IServiceCollection services,
        IConfiguration section,
        string configKey,
        bool register)
        where TDbContext : DbContext
        where TUnitOfWorkConcrete : EntityFrameworkUnitOfWork, TUnitOfWorkContract
        where TUnitOfWorkContract : class, IUnitOfWork
    {
        if (!register) {
            return;
        }

        var connectionString = section
            .GetRequiredSection(configKey)
            .Value ?? throw new InvalidOperationException($"Missing connection string {configKey}.");

        services.AddDbContext<TDbContext>((sp, options) => {
            var timestampInterceptor = sp.GetRequiredService<TimestampInterceptor>();
            options.UseSqlServer(connectionString);
            options.AddInterceptors(timestampInterceptor);
        });

        services.AddScoped<TUnitOfWorkContract, TUnitOfWorkConcrete>();
    }
}
