using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.NodeApi.Client;

/// <exclude/>
[ExcludeFromCodeCoverage]
public static class TokenCredentialHelpers
{
    /// <exclude/>
    public static TokenCredential CreateFromConfiguration(IConfiguration section)
    {
        return new ClientSecretCredential(
            tenantId: section.GetValue<string>("Tenant"),
            clientId: section.GetValue<string>("ClientId"),
            clientSecret: section.GetValue<string>("ClientSecret"),
            new TokenCredentialOptions {
                AuthorityHost = section.GetValue<Uri>("HostUrl"),
            }
        );
    }
}
