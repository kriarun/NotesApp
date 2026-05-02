using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using RpaIntegration.Api.Options;

namespace RpaIntegration.Api.Services;

public class CertificateService : ICertificateService
{
    private readonly KeycloakOptions _options;

    public CertificateService(IOptions<KeycloakOptions> options)
    {
        _options = options.Value;
    }

    public X509Certificate2 LoadCertificate()
    {
        // Step 1 — decode base64 to bytes
        var certBytes = Convert.FromBase64String(_options.CertificateBase64);

        // Step 2 — load certificate from bytes + password
        return new X509Certificate2(
            certBytes,
            _options.CertificatePassword,
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.Exportable);
    }
}