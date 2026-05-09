using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using RpaIntegration.Api.Options;
using RpaIntegration.Api.Services;
using Xunit;

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
            TokenUrl            = "https://keycloak.example.com/token",
            Audience            = "https://api.example.com",          // ← separate field
            ClientId            = "test-client",
            CertificateBase64   = "placeholder",
            CertificatePassword = "placeholder",
        });

        _sut = new ClientAssertionService(_mockCertService.Object, options);
    }

    private static X509Certificate2 CreateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "cn=test", rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        var cert = req.CreateSelfSigned(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1));
        // .NET 8 and below — use the constructor overload instead
        return new X509Certificate2(cert.Export(X509ContentType.Pfx));
    }

    [Fact]
    public void GetClientAssertion_ReturnsNonEmptyString()
    {
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate());

        var result = _sut.GetClientAssertion();

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void GetClientAssertion_ReturnsValidJwtStructure()
    {
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate());

        var result = _sut.GetClientAssertion();

        // JWT always has exactly 3 parts: header.payload.signature
        Assert.Equal(3, result.Split('.').Length);
    }

    [Fact]
    public void GetClientAssertion_ContainsCorrectIssuerAndSubject()
    {
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate());

        var result = _sut.GetClientAssertion();

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result);
        Assert.Equal("test-client", token.Issuer);
        Assert.Equal("test-client", token.Subject);
    }

    [Fact]
    public void GetClientAssertion_ContainsCorrectAudience()
    {
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate());

        var result = _sut.GetClientAssertion();

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result);

        // Audience comes from options.Audience, not TokenUrl
        Assert.Contains("https://api.example.com", token.Audiences);
    }

    [Fact]
    public void GetClientAssertion_AudienceIsNotTokenUrl()
    {
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate());

        var result = _sut.GetClientAssertion();

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result);

        // Pins the separation — TokenUrl must never bleed into the JWT audience
        Assert.DoesNotContain("https://keycloak.example.com/token", token.Audiences);
    }

    [Fact]
    public void GetClientAssertion_ExpiresInFiveMinutes()
    {
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate());

        var result = _sut.GetClientAssertion();

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result);
        Assert.True(token.ValidTo > DateTime.UtcNow);
        Assert.True(token.ValidTo <= DateTime.UtcNow.AddMinutes(5));
    }

    [Fact]
    public void GetClientAssertion_JtiIsUpperCase()
    {
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate());

        var result = _sut.GetClientAssertion();

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result);
        var jti = token.Claims.First(c => c.Type == "jti").Value;
        Assert.Equal(jti.ToUpperInvariant(), jti);
    }

    [Fact]
    public void GetClientAssertion_EachCallProducesUniqueJti()
    {
        // Factory delegate — fresh cert each call
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate);

        var jwt1 = _sut.GetClientAssertion();
        var jwt2 = _sut.GetClientAssertion();

        var handler = new JsonWebTokenHandler();
        var jti1 = handler.ReadJsonWebToken(jwt1).Claims.First(c => c.Type == "jti").Value;
        var jti2 = handler.ReadJsonWebToken(jwt2).Claims.First(c => c.Type == "jti").Value;

        Assert.NotEqual(jti1, jti2);
    }

    [Fact]
    public void GetClientAssertion_CallsCertificateServiceOnce()
    {
        _mockCertService.Setup(s => s.LoadCertificate()).Returns(CreateTestCertificate());

        _sut.GetClientAssertion();

        _mockCertService.Verify(s => s.LoadCertificate(), Times.Once);
    }

    [Fact]
    public void GetClientAssertion_ThrowsWhenCertificateHasNoPrivateKey()
    {
        // Export public part only — no private key attached
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "cn=test", rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        var publicOnlyCert = new X509Certificate2(
            req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1))
               .Export(X509ContentType.Cert));

        _mockCertService.Setup(s => s.LoadCertificate()).Returns(publicOnlyCert);

        // Production code: ?? throw new Exception("No RSA private key found in certificate.")
        Assert.Throws<Exception>(() => _sut.GetClientAssertion());
    }
}