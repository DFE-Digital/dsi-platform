
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;
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
    /// <param name="addAuditUnitOfWork">Register Audit unit of work.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="section"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddUnitOfWorkEntityFrameworkServices(
        this IServiceCollection services,
        IConfiguration section,
        bool addDirectoriesUnitOfWork,
        bool addOrganisationsUnitOfWork,
        bool addAuditUnitOfWork)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(section, nameof(section));

        if (addDirectoriesUnitOfWork || addOrganisationsUnitOfWork || addAuditUnitOfWork) {
            services.TryAddSingleton(TimeProvider.System);
            services.AddScoped<IEntityFrameworkTransactionContext, EntityFrameworkTransactionContext>();
            services.Decorate<IInteractionDispatcher, ProtectTransactionInteractionDispatcher>();
            services.AddScoped<TimestampInterceptor>();
        }

        AddUnitOfWork<IUnitOfWorkDirectories, UnitOfWorkDirectories, DbDirectoriesContext>(
            services,
            section,
            "Directories",
            addDirectoriesUnitOfWork);

        AddUnitOfWork<IUnitOfWorkOrganisations, UnitOfWorkOrganisations, DbOrganisationsContext>(
            services,
            section,
            "Organisations",
            addOrganisationsUnitOfWork);

        AddUnitOfWork<IUnitOfWorkAudit, UnitOfWorkAudit, DbAuditContext>(
            services,
            section,
            "Audit",
            addAuditUnitOfWork);

        return services;
    }

    /// <summary>
    /// Adds a specific Entity Framework Core unit of work implementation to the dependency
    /// injection <see cref="IServiceCollection"/>. Configures the corresponding <typeparamref name="TDbContext"/>
    /// with a SQL Server connection string derived from configuration and attaches a <see cref="TimestampInterceptor"/>
    /// to automatically manage CreatedAt and UpdatedAt timestamps.
    /// </summary>
    /// <typeparam name="TUnitOfWorkContract">
    /// The interface type representing the unit of work contract.
    /// </typeparam>
    /// <typeparam name="TUnitOfWorkConcrete">
    /// The concrete implementation type of the unit of work. Must inherit from
    /// <see cref="EntityFrameworkUnitOfWork"/> and implement <typeparamref name="TUnitOfWorkContract"/>.
    /// </typeparam>
    /// <typeparam name="TDbContext">
    /// The type of <see cref="DbContext"/> associated with this unit of work.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to register the unit of work and DbContext with.
    /// </param>
    /// <param name="section">
    /// The <see cref="IConfiguration"/> section containing the connection settings for this unit of work.
    /// Must contain keys: <c>Host</c>, <c>Name</c>, <c>Username</c>, and <c>Password</c>.
    /// </param>
    /// <param name="configKey">
    /// The configuration prefix key (e.g., "Directories" or "Organisations") used to locate the correct
    /// settings within <paramref name="section"/>.
    /// </param>
    /// <param name="register">
    /// If <c>true</c>, the unit of work and DbContext will be registered.
    /// If <c>false</c>, the method returns immediately without registering.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///   <para>Thrown if any required configuration value
    ///   (<c>Host</c>, <c>Name</c>, <c>Username</c>, <c>Password</c>)
    ///   is missing for the specified <paramref name="configKey"/>.</para>
    /// </exception>
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

        var dbHost = section.GetRequiredSection($"{configKey}:Host").Value;

        var dbName = section.GetRequiredSection($"{configKey}:Name").Value;

        var dbUsername = section.GetRequiredSection($"{configKey}:Username").Value;

        var dbPassword = section.GetRequiredSection($"{configKey}:Password").Value;

        var connectionBuilder = new SqlConnectionStringBuilder {
            DataSource = dbHost,
            InitialCatalog = dbName,
            UserID = dbUsername,
            Password = dbPassword,
            Encrypt = true,
            TrustServerCertificate = true,
            ConnectTimeout = 15,
        };

        services.AddDbContext<TDbContext>((sp, options) => {
            var timestampInterceptor = sp.GetRequiredService<TimestampInterceptor>();
            options.UseSqlServer(connectionBuilder.ConnectionString, sqlOptions => {
                sqlOptions.EnableRetryOnFailure();
            });
            options.AddInterceptors(timestampInterceptor);
        });

        services.AddScoped<TUnitOfWorkContract, TUnitOfWorkConcrete>();
    }
}
