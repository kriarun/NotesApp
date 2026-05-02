using System.Net.Http.Json;
using RpaIntegration.Api.Models;

namespace RpaIntegration.Api.Services;

public class ContractService : IContractService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ContractService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("TargetApiClient");
    }

    public async Task<Contract> CreateContractAsync(Contract contract)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/contracts", contract);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Contract>()
               ?? throw new InvalidOperationException("No contract in response");
    }

    public async Task<Contract> GetContractAsync(string id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"/api/contracts/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Contract>()
               ?? throw new InvalidOperationException("No contract in response");
    }

    public async Task<ActivationCode> SendActivationCodeAsync(string contractId)
    {
        var client = CreateClient();
        var response = await client.PostAsync(
            $"/api/contracts/{contractId}/activation", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ActivationCode>()
               ?? throw new InvalidOperationException("No activation code in response");
    }

    public async Task DeleteActivationCodeAsync(string contractId)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync(
            $"/api/contracts/{contractId}/activation");
        response.EnsureSuccessStatusCode();
    }

    public async Task<Letter> SendLetterAsync(string contractId, Letter letter)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(
            $"/api/contracts/{contractId}/letters", letter);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Letter>()
               ?? throw new InvalidOperationException("No letter in response");
    }
}