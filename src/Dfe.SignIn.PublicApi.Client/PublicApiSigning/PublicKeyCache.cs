using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.SignIn.PublicApi.Client.PublicApiSigning;

internal sealed class PublicKeyCache(
    IOptions<DfePublicApiOptions> publicApiOptionsAccessor,
    IOptions<PublicKeyCacheOptions> cacheOptionsAccessor,
    IHttpClientFactory httpClientFactory,
    ILogger<PublicKeyCache> logger
) : IPublicKeyCache
{
    // Options cannot be changed after initialisation.
    private readonly PublicKeyCacheOptions cacheOptions = cacheOptionsAccessor.Value;

    private Dictionary<string, PublicKeyCacheEntry> publicKeys = [];
    private DateTime refreshTime = new(0, DateTimeKind.Utc);
    private DateTime refreshRequestTime = new(0, DateTimeKind.Utc);

    private readonly object acquireRefreshLock = new();
    private bool isRefreshing = false;

    private bool IsCacheStale => DateTime.UtcNow - this.refreshTime > this.cacheOptions.TTL;
    private TimeSpan TimeSinceLastRefreshed => DateTime.UtcNow - this.refreshRequestTime;
    private bool HasRefreshedRecently => this.TimeSinceLastRefreshed <= this.cacheOptions.MaximumRefreshInterval;

    /// <inheritdoc/>
    public async Task<PublicKeyCacheEntry?> GetPublicKeyAsync(string keyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId, nameof(keyId));

        await this.AutoRefreshAsync(keyId);

        this.publicKeys.TryGetValue(keyId, out var result);
        return result;
    }

    private async Task AutoRefreshAsync(string keyId)
    {
        if (this.HasRefreshedRecently) {
            // Do not attempt to automatically refresh if a refresh was already requested recently.
            //   - Avoid the same keys being requested multiple times concurrently.
            //   - Avoid refreshing too frequently when unexpected key IDs are being encountered.
            return;
        }

        // Avoid multiple threads attempting to acquire a refresh concurrently.
        lock (this.acquireRefreshLock) {
            if (this.isRefreshing) {
                return;
            }
            this.isRefreshing = true;
        }

        try {
            bool wasUnknownKeyRequested = !this.publicKeys.ContainsKey(keyId);
            if (wasUnknownKeyRequested) {
                // Refresh cache and await so that we can see if we have encountered a new key.
                await this.RefreshAsync();
            }
            else if (this.IsCacheStale) {
                // Refresh cache asynchronously; we don't need to await since we already have the key.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.RefreshAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
        finally {
            this.isRefreshing = false;
        }
    }

    private async Task RefreshAsync()
    {
        try {
            this.refreshRequestTime = DateTime.UtcNow;

            var keysListing = await this.FetchPublicKeysAsync();

            this.publicKeys = keysListing.Keys
                // Exclude keys that have expired.
                .Where(key => DateTime.UtcNow <= DateTime.UnixEpoch.AddSeconds(key.Ed))
                .ToDictionary(key => key.Kid, key => {
                    // Retain existing key instance if it already exists.
                    if (this.publicKeys.TryGetValue(key.Kid, out var existingKey)) {
                        return existingKey;
                    }
                    // Read new public key.
                    return ReadPublicKey(key);
                });

            this.refreshTime = DateTime.UtcNow;
        }
        catch (Exception ex) {
            // Will continue to use public keys that have already been cached for
            // up to `MaximumRequestInterval` and then will try again.
            logger.LogWarning(ex, "Was unable to retrieve public keys from DfE Sign-in Public API.");
            return;
        }
    }

    private async Task<WellKnownPublicKeyListing> FetchPublicKeysAsync()
    {
        var httpClient = httpClientFactory.CreateClient(DfePublicApiConstants.HttpClientKey);

        var requestUri = new Uri(publicApiOptionsAccessor.Value.BaseAddress, "v2/.well-known/keys");
        var response = await httpClient.GetFromJsonAsync<WellKnownPublicKeyListing>(requestUri);
        if (response is null || !response.Keys.Any()) {
            throw new NoPublicKeysWereFoundException();
        }
        return response;
    }

    private static PublicKeyCacheEntry ReadPublicKey(WellKnownPublicKey publicKey)
    {
        var rsa = RSA.Create(new RSAParameters {
            Modulus = Base64UrlEncoder.DecodeBytes(publicKey.N),
            Exponent = Base64UrlEncoder.DecodeBytes(publicKey.E),
        });
        return new PublicKeyCacheEntry(publicKey, rsa);
    }
}
