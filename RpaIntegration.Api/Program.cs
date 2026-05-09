using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using RpaIntegration.Api.Options;
using RpaIntegration.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Options
builder.Services.AddOptions<KeycloakOptions>()
    .Bind(builder.Configuration.GetSection("KeycloakOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<TargetApiOptions>()
    .Bind(builder.Configuration.GetSection("TargetApiOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Services
builder.Services.AddSingleton<ICertificateService, CertificateService>();
builder.Services.AddScoped<IContractService, ContractService>();

var useMock = builder.Configuration.GetValue<bool>("UseMockTokenService");

if (useMock)
{
    // Mock mode — plain HttpClient, no Duende token management
    builder.Services.AddScoped<ITokenService, MockTokenService>();
    builder.Services.AddHttpClient("TargetApiClient", client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["TargetApiOptions:BaseUrl"]!);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}
else
{
    // Real mode — Duende manages tokens automatically
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddSingleton<IClientAssertionService, ClientAssertionService>();

    builder.Services.AddClientCredentialsTokenManagement()
        .AddClient(ClientCredentialsClientName.Parse("keycloak"), client =>
        {
            client.TokenEndpoint = new Uri(
                builder.Configuration["KeycloakOptions:TokenUrl"]!);
            client.ClientId = ClientId.Parse(
                builder.Configuration["KeycloakOptions:ClientId"]!);
            client.ClientCredentialStyle = ClientCredentialStyle.PostBody;
        });

    builder.Services.AddClientCredentialsHttpClient(
        "TargetApiClient",
        ClientCredentialsClientName.Parse("keycloak"),
        client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["TargetApiOptions:BaseUrl"]!);
        })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

namespace RpaIntegration.Api
{
    public partial class Program { }
}