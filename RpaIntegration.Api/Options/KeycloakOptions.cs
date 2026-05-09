using System.ComponentModel.DataAnnotations;

namespace RpaIntegration.Api.Options;

public class KeycloakOptions
{
    public string TokenUrl { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string CertificateBase64 { get; set; } = string.Empty;
    public string CertificatePassword { get; set; } = string.Empty;
}