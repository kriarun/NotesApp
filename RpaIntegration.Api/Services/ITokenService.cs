namespace RpaIntegration.Api.Services;

public interface ITokenService
{
    Task<string> GetAccessTokenAsync();
}