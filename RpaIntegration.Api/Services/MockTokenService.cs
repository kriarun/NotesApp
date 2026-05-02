namespace RpaIntegration.Api.Services;

public class MockTokenService : ITokenService
{
    public Task<string> GetAccessTokenAsync()
    {
        // Returns a fake token — no Keycloak needed
        return Task.FromResult("mock-access-token-for-testing");
    }
}