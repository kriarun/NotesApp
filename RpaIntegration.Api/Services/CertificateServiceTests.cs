using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using RpaIntegration.Api.Options;
using RpaIntegration.Api.Services;
using Xunit;

namespace NotesApp.Tests;

public class CertificateServiceTests
{
    private readonly string _testCertBase64;
    private const string TestPassword = "test-password";

    public CertificateServiceTests()
    {
        // Generate self-signed cert in memory
        using var rsa = RSA.Create(2048);
        var certRequest = new CertificateRequest(
            "cn=test", rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        var cert = certRequest.CreateSelfSigned(
            DateTimeOffset.Now,
            DateTimeOffset.Now.AddYears(1));

        _testCertBase64 = Convert.ToBase64String(
            cert.Export(X509ContentType.Pfx, TestPassword));
    }

    private CertificateService CreateService(
        string? base64 = null,
        string? password = null)
    {
        var options = Options.Create(new KeycloakOptions
        {
            TokenUrl = "https://keycloak.example.com/token",
            ClientId = "test-client",
            CertificateBase64 = base64 ?? _testCertBase64,
            CertificatePassword = password ?? TestPassword
        });

        return new CertificateService(options);
    }

    [Fact]
    public void LoadCertificate_WithValidBase64_ReturnsCertificate()
    {
        // Arrange
        var service = CreateService();

        // Act
        var cert = service.LoadCertificate();

        // Assert
        Assert.NotNull(cert);
        Assert.True((bool)cert.HasPrivateKey);
    }

    [Fact]
    public void LoadCertificate_WithValidBase64_CertificateNotDisposed()
    {
        // Arrange
        var service = CreateService();

        // Act
        var cert = service.LoadCertificate();

        // Assert — use cert after return — should not throw disposed exception
        var exception = Record.Exception(() =>
        {
            var key = cert.GetRSAPrivateKey();
            Assert.NotNull(key);
        });

        Assert.Null(exception); // ← no disposed exception ✅
    }

    [Fact]
    public void LoadCertificate_WithInvalidBase64_ThrowsFormatException()
    {
        // Arrange
        var service = CreateService(base64: "not-valid-base64!!!");

        // Act & Assert
        void Act() => service.LoadCertificate();
        Assert.Throws<FormatException>(Act);
    }

    [Fact]
    public void LoadCertificate_WithWrongPassword_ThrowsCryptographicException()
    {
        // Arrange
        var service = CreateService(password: "wrong-password");

        // Act & Assert
        void Act() => service.LoadCertificate();
        Assert.Throws<CryptographicException>(Act);
    }

    [Fact]
    public void LoadCertificate_CalledMultipleTimes_NeverThrowsDisposedException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert — simulate token refresh scenario
        for (int i = 0; i < 5; i++)
        {
            var exception = Record.Exception(() =>
            {
                var cert = service.LoadCertificate();
                var key = cert.GetRSAPrivateKey();
                Assert.NotNull(key);
            });

            Assert.Null(exception); // ← no disposed exception on repeated calls ✅
        }
    }
}
