
using Dfe.SignIn.Base.Framework;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration;

/// <summary>
/// Extension methods for setting up the Entity Framework databases.
/// </summary>
public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Adds the required configuration for using Entity Framework Core.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="section">Configuration section.</param>
    /// <param name="addDirectories">Register Directories database.</param>
    /// <param name="addOrganisations">Register Organisations database.</param>
    /// <param name="addAudit">Register Audit database.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="section"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddEntityFrameworkServices(
        this IServiceCollection services,
        IConfiguration section,
        bool addDirectories,
        bool addOrganisations,
        bool addAudit)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(section, nameof(section));

        if (addDirectories || addOrganisations || addAudit) {
            services.TryAddSingleton(TimeProvider.System);
            services.AddScoped<TimestampInterceptor>();
        }

        ConfigureDatabase<DbDirectoriesContext>(
            services,
            section,
            "Directories",
            addDirectories);

        ConfigureDatabase<DbOrganisationsContext>(
            services,
            section,
            "Organisations",
            addOrganisations);

        ConfigureDatabase<DbAuditContext>(
            services,
            section,
            "Audit",
            addAudit);

        return services;
    }

    /// <summary>
    /// Adds a specific Entity Framework Core unit of work implementation to the dependency
    /// injection <see cref="IServiceCollection"/>. Configures the corresponding <typeparamref name="TDbContext"/>
    /// with a SQL Server connection string derived from configuration and attaches a <see cref="TimestampInterceptor"/>
    /// to automatically manage CreatedAt and UpdatedAt timestamps.
    /// </summary>
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
    private static void ConfigureDatabase<TDbContext>(
        IServiceCollection services,
        IConfiguration section,
        string configKey,
        bool register)
        where TDbContext : DbContext
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
            options.UseSqlServer(connectionBuilder.ConnectionString, sqlOptions => {
                sqlOptions.EnableRetryOnFailure();
            });

            var timestampInterceptor = sp.GetRequiredService<TimestampInterceptor>();
            options.AddInterceptors(timestampInterceptor);

            foreach (var commandInterceptor in sp.GetServices<DbCommandInterceptor>()) {
                options.AddInterceptors(commandInterceptor);
            }
        });
    }
}
