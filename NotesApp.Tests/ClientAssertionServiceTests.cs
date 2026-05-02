using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Duende.AccessTokenManagement;
using Microsoft.Extensions.Options;
using Moq;
using RpaIntegration.Api.Options;
using RpaIntegration.Api.Services;

namespace NotesApp.Tests;

public class ClientAssertionServiceTests
{
    private readonly Mock<ICertificateService> _mockCertService;
    private readonly ClientAssertionService _sut;

    public ClientAssertionServiceTests()
    {
        _mockCertService = new Mock<ICertificateService>();

        var options = Options.Create(new KeycloakOptions
        {
            TokenUrl = "https://keycloak.example.com/token",
            ClientId = "test-client",
            CertificateBase64 = "placeholder",
            CertificatePassword = "placeholder"
        });

        _sut = new ClientAssertionService(_mockCertService.Object, options);
    }

    private static X509Certificate2 CreateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var certRequest = new CertificateRequest(
            "cn=test", rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return certRequest.CreateSelfSigned(
            DateTimeOffset.Now,
            DateTimeOffset.Now.AddYears(1));
    }

    [Fact]
    public async Task GetClientAssertionAsync_ReturnsAssertion()
    {
        // Arrange
        var cert = CreateTestCertificate();
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(cert);

        // Act
        var result = await _sut.GetClientAssertionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            result.Type);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public async Task GetClientAssertionAsync_AssertionIsValidJwt()
    {
        // Arrange
        var cert = CreateTestCertificate();
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(cert);

        // Act
        var result = await _sut.GetClientAssertionAsync();

        // Assert — JWT has 3 parts separated by dots
        var parts = result!.Value.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public async Task GetClientAssertionAsync_CallsCertificateService()
    {
        // Arrange
        var cert = CreateTestCertificate();
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(cert);

        // Act
        await _sut.GetClientAssertionAsync();

        // Assert — verify CertificateService was called exactly once
        _mockCertService.Verify(s => s.LoadCertificate(), Times.Once);
    }
}