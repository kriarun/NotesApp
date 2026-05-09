using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RpaIntegration.Api.Options;

namespace RpaIntegration.Api.Services;

public class ClientAssertionService
{
    private readonly ICertificateService _certificateService;
    private readonly IOptions<KeycloakOptions> _options;

    public ClientAssertionService(
        ICertificateService certificateService,
        IOptions<KeycloakOptions> options)
    {
        _certificateService = certificateService;
        _options = options;
    }

    public string GetClientAssertion()
    {
        var options = _options.Value;
        var certificate = _certificateService.LoadCertificate();
        var rsaPrivateKey = certificate.GetRSAPrivateKey()
            ?? throw new Exception("No RSA private key found in certificate.");

        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(rsaPrivateKey),
            SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = options.ClientId,
            Audience = options.Audience,          // ← Audience, not TokenUrl
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = signingCredentials,
            Claims = new Dictionary<string, object>
            {
                { "sub", options.ClientId },
                { "jti", Guid.NewGuid().ToString().ToUpper(CultureInfo.InvariantCulture) },
            },
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
}