using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Moq;
using RpaIntegration.Api.Options;
using RpaIntegration.Api.Services;

namespace NotesApp.Tests;

public class DynamicAssertionTokenServiceTests
{
    private readonly Mock<ICertificateService> _mockCertService;
    private readonly ClientAssertionService _assertionService;
    private readonly DynamicAssertionTokenService _sut;

    public DynamicAssertionTokenServiceTests()
    {
        _mockCertService = new Mock<ICertificateService>();

        var options = Options.Create(new KeycloakOptions
        {
            TokenUrl           = "https://keycloak.example.com/token",
            Audience           = "https://api.example.com",
            ClientId           = "test-client",
            CertificateBase64  = "placeholder",
            CertificatePassword = "placeholder",
        });

        _assertionService = new ClientAssertionService(_mockCertService.Object, options);
        _sut = new DynamicAssertionTokenService(_assertionService, options);

        _mockCertService
            .Setup(s => s.LoadCertificate())
            .Returns(CreateTestCertificate);   // factory — fresh cert per call
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
        return X509Certificate2.CreateFromPkcs12(cert.Export(X509ContentType.Pfx));
    }

    // ── GetClientCredentialsRequestAsync — if-branch coverage ────────────────

    [Fact]
    public async Task GetClientCredentialsRequestAsync_MatchingClientName_SetsClientAssertion()
    {
        var result = await _sut.GetClientCredentialsRequestAsync(
            "rpa-access-token", new ClientAccessTokenParameters());

        Assert.NotNull(result.ClientAssertion);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_WrongClientName_DoesNotSetClientAssertion()
    {
        var result = await _sut.GetClientCredentialsRequestAsync(
            "some-other-client", new ClientAccessTokenParameters());

        Assert.Null(result.ClientAssertion);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_ClientNameIsTheOnlyDifference()
    {
        // Shows the if-branch clearly — same service, only client name differs
        var matched = await _sut.GetClientCredentialsRequestAsync(
            "rpa-access-token", new ClientAccessTokenParameters());

        var unmatched = await _sut.GetClientCredentialsRequestAsync(
            "some-other-client", new ClientAccessTokenParameters());

        Assert.NotNull(matched.ClientAssertion);    // ← if branch taken
        Assert.Null(unmatched.ClientAssertion);     // ← if branch skipped
    }

    // ── GetClientCredentialsRequestAsync — assertion content ─────────────────

    [Fact]
    public async Task GetClientCredentialsRequestAsync_MatchingClientName_SetsCorrectAssertionType()
    {
        var result = await _sut.GetClientCredentialsRequestAsync(
            "rpa-access-token", new ClientAccessTokenParameters());

        Assert.Equal(
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            result.ClientAssertion!.Type);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_MatchingClientName_SetsValidJwtAsValue()
    {
        var result = await _sut.GetClientCredentialsRequestAsync(
            "rpa-access-token", new ClientAccessTokenParameters());

        // JWT always has exactly 3 parts: header.payload.signature
        Assert.Equal(3, result.ClientAssertion!.Value.Split('.').Length);
    }

    // ── GetClientCredentialsRequestAsync — request fields ────────────────────

    [Fact]
    public async Task GetClientCredentialsRequestAsync_SetsTokenUrlAsAddress()
    {
        var result = await _sut.GetClientCredentialsRequestAsync(
            "rpa-access-token", new ClientAccessTokenParameters());

        Assert.Equal("https://keycloak.example.com/token", result.Address);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_SetsClientId()
    {
        var result = await _sut.GetClientCredentialsRequestAsync(
            "rpa-access-token", new ClientAccessTokenParameters());

        Assert.Equal("test-client", result.ClientId);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_SetsGrantType()
    {
        var result = await _sut.GetClientCredentialsRequestAsync(
            "rpa-access-token", new ClientAccessTokenParameters());

        Assert.Equal("client_credentials", result.GrantType);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_SetsClientCredentialStyleToPostBody()
    {
        var result = await _sut.GetClientCredentialsRequestAsync(
            "rpa-access-token", new ClientAccessTokenParameters());

        Assert.Equal(ClientCredentialStyle.PostBody, result.ClientCredentialStyle);
    }

    // ── GetRefreshTokenRequestAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetRefreshTokenRequestAsync_ReturnsNonNull()
    {
        var result = await _sut.GetRefreshTokenRequestAsync(
            new UserAccessTokenParameters());

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetRefreshTokenRequestAsync_ReturnsRefreshTokenRequest()
    {
        var result = await _sut.GetRefreshTokenRequestAsync(
            new UserAccessTokenParameters());

        Assert.IsType<RefreshTokenRequest>(result);
    }

    // ── GetTokenRevocationRequestAsync ───────────────────────────────────────

    [Fact]
    public async Task GetTokenRevocationRequestAsync_ReturnsNonNull()
    {
        var result = await _sut.GetTokenRevocationRequestAsync(
            new UserAccessTokenParameters());

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTokenRevocationRequestAsync_ReturnsTokenRevocationRequest()
    {
        var result = await _sut.GetTokenRevocationRequestAsync(
            new UserAccessTokenParameters());

        Assert.IsType<TokenRevocationRequest>(result);
    }
}