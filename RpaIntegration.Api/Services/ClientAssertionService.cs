using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RpaIntegration.Api.Options;

namespace RpaIntegration.Api.Services;

public class ClientAssertionService : IClientAssertionService
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

    public Task<ClientAssertion?> GetClientAssertionAsync(
        ClientCredentialsClientName? clientName = null,
        TokenRequestParameters? parameters = null,
        CancellationToken cancellationToken = default)  // ← add this
    {
        var options = _options.Value;
        var certificate = _certificateService.LoadCertificate();

        var securityKey = new X509SecurityKey(certificate);
        var signingCredentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = options.ClientId,
            Audience = options.TokenUrl,
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = signingCredentials,
            Claims = new Dictionary<string, object>
            {
                { "sub", options.ClientId },
                { "jti", Guid.NewGuid().ToString() }
            }
        };

        var tokenHandler = new JsonWebTokenHandler();
        var assertion = new ClientAssertion
        {
            Type = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            Value = tokenHandler.CreateToken(tokenDescriptor)
        };

        return Task.FromResult<ClientAssertion?>(assertion);
    }
}