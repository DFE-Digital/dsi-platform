using Azure.Core;
using Azure.Security.KeyVault.Keys;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.WebFramework.Mvc;

/// <summary>
/// Extension methods for configuring ASP.NET Core Data Protection with multiple storage strategies and optional Key Vault encryption.
/// </summary>
public static class DataProtectionExtensions
{
    /// <summary>
    /// Configures Data Protection for the application using the provided configuration and token credential.
    /// Supports multiple key storage strategies, including Blob Storage, File System, and optional Azure Key Vault encryption.
    /// </summary>
    /// <param name="services">The service collection to add Data Protection to.</param>
    /// <param name="configuration">The application configuration containing a "DataProtection" section.</param>
    /// <param name="tokenCredential">The Azure <see cref="TokenCredential"/> to use for authentication with Azure services (Blob Storage or Key Vault).</param>
    /// <param name="applicationName">The application name to isolate the Data Protection keys.</param>
    /// <returns>
    ///   <para>The original <see cref="IServiceCollection"/> for chaining.</para>
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///  <para>Thrown if the configured strategy is unsupported</para>
    ///  <para>- or -</para>
    ///  <para>Thrown BlobStorage is used but not configured.</para>
    ///  <para>- or -</para>
    ///  <para>Thrown FileSystem is used but not configured.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configuration"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="tokenCredential"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="applicationName"/> is null or empty.</para>
    /// </exception>
    public static IServiceCollection AddDsiDataProtection(this IServiceCollection services, IConfiguration configuration, TokenCredential tokenCredential, string applicationName)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));
        ExceptionHelpers.ThrowIfArgumentNull(tokenCredential, nameof(tokenCredential));
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(applicationName, nameof(applicationName));

        var dataProtectionSection = configuration.GetSection("DataProtection");
        if (!dataProtectionSection.Exists()) {
            // Data Protection has not bee configured; do nothing.
            return services;
        }

        var options = Activator.CreateInstance<Configuration.DataProtectionOptions>();
        dataProtectionSection.Bind(options);

        var dataProtectionBuilder = services.AddDataProtection()
            .SetApplicationName(applicationName)
            .SetDefaultKeyLifetime(TimeSpan.FromDays(options.KeyLifetimeInDays));

        switch (options.Strategy) {
            case "BlobStorage":
                if (options.BlobStorage is null) {
                    throw new InvalidOperationException("BlobStorage options must be provided when Strategy is 'BlobStorage'.");
                }
                ConfigureBlobStorage(dataProtectionBuilder, options.BlobStorage, tokenCredential);
                break;
            case "FileSystem":
                if (options.FileSystem is null) {
                    throw new InvalidOperationException("FileSystem options must be provided when Strategy is 'FileSystem'.");
                }
                ConfigureFileSystem(dataProtectionBuilder, options.FileSystem);
                break;
            default:
                throw new InvalidOperationException($"Unsupported data protection strategy: {options.Strategy}");
        }

        ConfigureKeyVaultProtection(dataProtectionBuilder, options.KeyVault, tokenCredential);

        return services;
    }

    /// <summary>
    /// Configures Data Protection to persist keys to the local file system.
    /// </summary>
    /// <param name="builder">The Data Protection builder.</param>
    /// <param name="options">The file system options.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="options"/> is null.</para>
    /// </exception>
    private static void ConfigureFileSystem(IDataProtectionBuilder builder, FileSystemOptions options)
    {
        ExceptionHelpers.ThrowIfArgumentNull(options, nameof(options));

        builder.PersistKeysToFileSystem(new DirectoryInfo(options.Path));
    }

    /// <summary>
    /// Configures Data Protection to persist keys to Azure Blob Storage.
    /// </summary>
    /// <param name="builder">The Data Protection builder.</param>
    /// <param name="options">The blob storage options.</param>
    /// <param name="tokenCredential">The Azure token credential for authentication.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="options"/> is null.</para>
    /// </exception>
    private static void ConfigureBlobStorage(IDataProtectionBuilder builder, BlobStorageOptions options, TokenCredential tokenCredential)
    {
        ExceptionHelpers.ThrowIfArgumentNull(options, nameof(options));

        builder.PersistKeysToAzureBlobStorage(new Uri(options.StorageAccountUri), tokenCredential);
    }

    /// <summary>
    /// Optionally configures Azure Key Vault encryption for the Data Protection keys.
    /// </summary>
    /// <param name="builder">The Data Protection builder.</param>
    /// <param name="options">The Key Vault options, including Vault URI and key name. If null, no Key Vault encryption is applied.</param>
    /// <param name="tokenCredential">The Azure token credential for authentication.</param>
    private static void ConfigureKeyVaultProtection(IDataProtectionBuilder builder, KeyVaultOptions? options, TokenCredential tokenCredential)
    {
        if (options is null) {
            return;
        }

        var keyClient = new KeyClient(new Uri(options.VaultUri), tokenCredential);
        var key = keyClient.GetKey(options.KeyName).Value;

        builder.ProtectKeysWithAzureKeyVault(key.Id.ToString(), tokenCredential);
    }
}
