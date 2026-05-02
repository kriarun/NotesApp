namespace TargetApi.Mock.Models;

public class ActivationCode
{
    public string Code { get; set; } = string.Empty;
    public string ContractId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
}