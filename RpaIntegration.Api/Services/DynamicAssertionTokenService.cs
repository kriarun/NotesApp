
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using RpaIntegration.Api.Options;
using RpaIntegration.Api.Services;

namespace RpaIntegration.Api.Services;

public class DynamicAssertionTokenService : ITokenClientConfigurationService
{
    private const string AssertionType =
        "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

    private const string ClientName = "rpa-access-token"; // ← rename to match your appsettings

    private readonly ClientAssertionService _assertionService;
    private readonly IOptions<KeycloakOptions> _options;

    public DynamicAssertionTokenService(
        ClientAssertionService assertionService,
        IOptions<KeycloakOptions> options)
    {
        _assertionService = assertionService;
        _options = options;
    }

    public Task<ClientCredentialsTokenRequest> GetClientCredentialsRequestAsync(
        string clientName,
        ClientAccessTokenParameters parameters)
    {
        var request = new ClientCredentialsTokenRequest
        {
            Address = _options.Value.TokenUrl,    // ← TokenUrl for the endpoint
            ClientId = _options.Value.ClientId,
            GrantType = "client_credentials",
            Scope = "openid",
            ClientCredentialStyle = ClientCredentialStyle.PostBody,
        };

        if (clientName == ClientName)
        {
            request.ClientAssertion = new ClientAssertion
            {
                Type = AssertionType,
                Value = _assertionService.GetClientAssertion(),
            };
        }

        return Task.FromResult(request);
    }

    public Task<RefreshTokenRequest> GetRefreshTokenRequestAsync(
        UserAccessTokenParameters parameters)
    {
        return Task.FromResult(new RefreshTokenRequest());
    }

    public Task<TokenRevocationRequest> GetTokenRevocationRequestAsync(
        UserAccessTokenParameters parameters)
    {
        return Task.FromResult(new TokenRevocationRequest());
    }
}