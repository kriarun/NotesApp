using System.ComponentModel.DataAnnotations;

namespace RpaIntegration.Api.Options;

public class KeycloakOptions
{
    [Required]
    public string TokenUrl { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string CertificateBase64 { get; set; } = string.Empty;  // ← changed

    [Required]
    public string CertificatePassword { get; set; } = string.Empty;
}