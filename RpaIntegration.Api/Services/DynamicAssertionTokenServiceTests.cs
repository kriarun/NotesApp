using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Moq;
using YourOrg.Options;
using YourOrg.Services;

namespace YourOrg.Tests;

public class DynamicAssertionTokenServiceTests
{
    private readonly Mock<ClientAssertionService> _mockAssertionService;
    private readonly DynamicAssertionTokenService _sut;

    public DynamicAssertionTokenServiceTests()
    {
        _mockAssertionService = new Mock<ClientAssertionService>();

        var options = Options.Create(new ShiftLightOptions
        {
            TokenUrl = "https://keycloak.example.com/token",
            ClientId = "test-client"
        });

        _sut = new DynamicAssertionTokenService(
            _mockAssertionService.Object,
            options);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_CorrectClientName_AddsClientAssertion()
    {
        // Arrange
        _mockAssertionService
            .Setup(s => s.GetClientAssertion())
            .Returns("fake-jwt-token");

        // Act
        var result = await _sut.GetClientCredentialsRequestAsync(
            "shiftlight-access-token",
            new ClientAccessTokenParameters());

        // Assert
        Assert.NotNull(result.ClientAssertion);
        Assert.Equal(
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            result.ClientAssertion.Type);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_CorrectClientName_UsesJwtValue()
    {
        // Arrange
        _mockAssertionService
            .Setup(s => s.GetClientAssertion())
            .Returns("fake-jwt-token");

        // Act
        var result = await _sut.GetClientCredentialsRequestAsync(
            "shiftlight-access-token",
            new ClientAccessTokenParameters());

        // Assert
        Assert.Equal("fake-jwt-token", result.ClientAssertion!.Value);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_WrongClientName_NoClientAssertion()
    {
        // Act
        var result = await _sut.GetClientCredentialsRequestAsync(
            "some-other-client", // ← wrong name
            new ClientAccessTokenParameters());

        // Assert — no assertion added for wrong client
        Assert.Null(result.ClientAssertion);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_CorrectClientName_UsesPostBody()
    {
        // Arrange
        _mockAssertionService
            .Setup(s => s.GetClientAssertion())
            .Returns("fake-jwt");

        // Act
        var result = await _sut.GetClientCredentialsRequestAsync(
            "shiftlight-access-token",
            new ClientAccessTokenParameters());

        // Assert
        Assert.Equal(
            ClientCredentialStyle.PostBody,
            result.ClientCredentialStyle);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_CorrectClientName_CallsGetClientAssertion()
    {
        // Arrange
        _mockAssertionService
            .Setup(s => s.GetClientAssertion())
            .Returns("fake-jwt");

        // Act
        await _sut.GetClientCredentialsRequestAsync(
            "shiftlight-access-token",
            new ClientAccessTokenParameters());

        // Assert — GetClientAssertion called exactly once
        _mockAssertionService.Verify(
            s => s.GetClientAssertion(),
            Times.Once);
    }

    [Fact]
    public async Task GetClientCredentialsRequestAsync_WrongClientName_NeverCallsGetClientAssertion()
    {
        // Act
        await _sut.GetClientCredentialsRequestAsync(
            "some-other-client",
            new ClientAccessTokenParameters());

        // Assert — GetClientAssertion never called for wrong client
        _mockAssertionService.Verify(
            s => s.GetClientAssertion(),
            Times.Never);
    }
}
