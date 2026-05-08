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
    public async Task GetClientAssertionAsync_ReturnsNotNull()
    {
        // Arrange
        _mockCertService
            .Setup(s => s.LoadCertificate())
            .Returns(CreateTestCertificate());

        // Act
        var result = await _sut.GetClientAssertionAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetClientAssertionAsync_ReturnsCorrectType()
    {
        // Arrange
        _mockCertService
            .Setup(s => s.LoadCertificate())
            .Returns(CreateTestCertificate());

        // Act
        var result = await _sut.GetClientAssertionAsync();

        // Assert
        Assert.Equal(
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            result!.Type);
    }

    [Fact]
    public async Task GetClientAssertionAsync_ReturnsValidJwt()
    {
        // Arrange
        _mockCertService
            .Setup(s => s.LoadCertificate())
            .Returns(CreateTestCertificate());

        // Act
        var result = await _sut.GetClientAssertionAsync();

        // Assert — JWT has 3 parts separated by dots
        var parts = result!.Value.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public async Task GetClientAssertionAsync_JwtContainsCorrectClaims()
    {
        // Arrange
        _mockCertService
            .Setup(s => s.LoadCertificate())
            .Returns(CreateTestCertificate());

        // Act
        var result = await _sut.GetClientAssertionAsync();

        // Assert — decode JWT and check claims
        var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        var token = handler.ReadJsonWebToken(result!.Value);

        Assert.Equal("test-client", token.Issuer);
        Assert.Equal("test-client", token.Subject);
        Assert.Contains("https://keycloak.example.com/token", token.Audiences);
    }

    [Fact]
    public async Task GetClientAssertionAsync_JwtExpiresInFiveMinutes()
    {
        // Arrange
        _mockCertService
            .Setup(s => s.LoadCertificate())
            .Returns(CreateTestCertificate());

        // Act
        var result = await _sut.GetClientAssertionAsync();

        // Assert
        var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        var token = handler.ReadJsonWebToken(result!.Value);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(5);
        Assert.True(token.ValidTo <= expectedExpiry);
        Assert.True(token.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public async Task GetClientAssertionAsync_CallsCertificateServiceOnce()
    {
        // Arrange
        _mockCertService
            .Setup(s => s.LoadCertificate())
            .Returns(CreateTestCertificate());

        // Act
        await _sut.GetClientAssertionAsync();

        // Assert
        _mockCertService.Verify(s => s.LoadCertificate(), Times.Once);
    }

    [Fact]
    public async Task GetClientAssertionAsync_CalledMultipleTimes_AlwaysReturnsFreshJwt()
    {
        // Arrange — simulates token refresh scenario
        _mockCertService
            .Setup(s => s.LoadCertificate())
            .Returns(CreateTestCertificate);  // ← factory, creates fresh cert each time

        // Act
        var result1 = await _sut.GetClientAssertionAsync();
        await Task.Delay(1000); // wait 1 second
        var result2 = await _sut.GetClientAssertionAsync();

        // Assert — different JTI each time (unique JWT)
        var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        var token1 = handler.ReadJsonWebToken(result1!.Value);
        var token2 = handler.ReadJsonWebToken(result2!.Value);

        Assert.NotEqual(
            token1.Claims.First(c => c.Type == "jti").Value,
            token2.Claims.First(c => c.Type == "jti").Value); // ← different JTI ✅
    }
}
