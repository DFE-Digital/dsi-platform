using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Core;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.SignIn.NodeApi.Client;

/// <summary>
/// Generates locally-signed JWT tokens for development.
/// Only used when the environment is "Local".
/// </summary>
public sealed class LocalTokenCredential : TokenCredential
{
    /// <inheritdoc/>
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(LocalDevAuth.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: LocalDevAuth.Issuer,
            audience: LocalDevAuth.Audience,
            claims: [new Claim("sub", "local-dev-caller")],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(tokenString, token.ValidTo);
    }

    /// <inheritdoc/>
    public override ValueTask<AccessToken> GetTokenAsync(
        TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(GetToken(requestContext, cancellationToken));
    }
}
