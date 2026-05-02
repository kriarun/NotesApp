using System.Security.Cryptography.X509Certificates;
using Duende.AccessTokenManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RpaIntegration.Api.Options;

namespace RpaIntegration.Api.Services;

public class TokenService : ITokenService
{
    private readonly KeycloakOptions _options;
    private readonly ICertificateService _certificateService;

    public TokenService(
        IOptions<KeycloakOptions> options,
        ICertificateService certificateService)
    {
        _options = options.Value;
        _certificateService = certificateService;
    }

    public Task<string> GetAccessTokenAsync()
    {
        var assertion = CreateClientAssertion();
        return Task.FromResult(assertion);
    }

    public string CreateClientAssertion()
    {
        // Step 1 — load certificate via CertificateService
        var certificate = _certificateService.LoadCertificate();

        // Step 2 — create signing credentials
        var securityKey = new X509SecurityKey(certificate);
        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.RsaSha256);

        // Step 3 — build JWT client assertion
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.ClientId,
            Audience = _options.TokenUrl,
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = signingCredentials,
            Claims = new Dictionary<string, object>
            {
                { "sub", _options.ClientId },
                { "jti", Guid.NewGuid().ToString() }
            }
        };

        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}