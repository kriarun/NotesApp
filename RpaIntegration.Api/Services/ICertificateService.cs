using System.Security.Cryptography.X509Certificates;

namespace RpaIntegration.Api.Services;

public interface ICertificateService
{
    X509Certificate2 LoadCertificate();
}