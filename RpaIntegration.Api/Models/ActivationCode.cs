namespace RpaIntegration.Api.Models;

public class ActivationCode
{
    public string Code { get; set; } = string.Empty;
    public string ContractId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}