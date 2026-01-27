namespace Dfe.SignIn.WebFramework.Mvc.Configuration;

/// <summary>
/// Configuration options for ASP.NET Core Data Protection.
/// Determines how keys are stored, their lifetime, and optional encryption.
/// </summary>
public sealed record DataProtectionOptions
{
    /// <summary>
    /// The storage strategy to use for Data Protection keys.
    /// Supported values: "BlobStorage", "FileSystem".
    /// </summary>
    public required string Strategy { get; init; }

    /// <summary>
    /// The lifetime of each Data Protection key in days.
    /// Defaults to 90 days if not specified.
    /// </summary>
    public required int KeyLifetimeInDays { get; init; } = 90;

    /// <summary>
    /// Options for storing keys in Azure Blob Storage.
    /// Required if <see cref="Strategy"/> is "BlobStorage".
    /// </summary>
    public BlobStorageOptions? BlobStorage { get; init; }

    /// <summary>
    /// Options for storing keys on the local file system.
    /// Required if <see cref="Strategy"/> is "FileSystem".
    /// </summary>
    public FileSystemOptions? FileSystem { get; init; }

    /// <summary>
    /// Optional configuration for encrypting keys with Azure Key Vault.
    /// </summary>
    public KeyVaultOptions? KeyVault { get; init; }
}

/// <summary>
/// Configuration options for storing Data Protection keys in Azure Blob Storage.
/// </summary>
public sealed record BlobStorageOptions
{
    /// <summary>
    /// The URI of the Azure Storage account containing the container.
    /// Example: "https://{storage_account}.blob.core.windows.net/{container}/{blob}".
    /// </summary>
    public required string StorageAccountUri { get; init; }
}

/// <summary>
/// Configuration options for storing Data Protection keys on the local file system.
/// </summary>
public sealed record FileSystemOptions
{
    /// <summary>
    /// The folder path where the keys will be stored.
    /// Example: "C:\Keys" or "/var/keys".
    /// </summary>
    public required string Path { get; init; }
}

/// <summary>
/// Configuration options for protecting Data Protection keys with Azure Key Vault.
/// </summary>
public sealed record KeyVaultOptions
{
    /// <summary>
    /// The URI of the Azure Key Vault.
    /// </summary>
    public required string VaultUri { get; init; }

    /// <summary>
    /// The name of the key in Azure Key Vault to use for encryption.
    /// </summary>
    public required string KeyName { get; init; }
}
