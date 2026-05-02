using RpaIntegration.Api.Models;

namespace RpaIntegration.Api.Services;

public interface IContractService
{
    Task<Contract> CreateContractAsync(Contract contract);
    Task<Contract> GetContractAsync(string id);
    Task<ActivationCode> SendActivationCodeAsync(string contractId);
    Task DeleteActivationCodeAsync(string contractId);
    Task<Letter> SendLetterAsync(string contractId, Letter letter);
}