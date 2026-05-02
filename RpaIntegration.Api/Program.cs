using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using RpaIntegration.Api.Options;
using RpaIntegration.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Options
builder.Services.AddOptions<KeycloakOptions>()
    .Bind(builder.Configuration.GetSection("KeycloakOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<TargetApiOptions>()
    .Bind(builder.Configuration.GetSection("TargetApiOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// HTTP Clients
// HTTP Clients
builder.Services.AddHttpClient("KeycloakClient");
builder.Services.AddHttpClient("TargetApiClient", client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["TargetApiOptions:BaseUrl"]!);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Dev only — ignore SSL errors between local projects
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Services

builder.Services.AddSingleton<ICertificateService, CertificateService>();

// Access Token Management
builder.Services.AddClientCredentialsTokenManagement()
    .AddClient(ClientCredentialsClientName.Parse("keycloak"), client =>
    {
        client.TokenEndpoint = new Uri(
            builder.Configuration["KeycloakOptions:TokenUrl"]!);
        client.ClientId = ClientId.Parse(
            builder.Configuration["KeycloakOptions:ClientId"]!);
        client.ClientCredentialStyle = ClientCredentialStyle.PostBody;
    });
    
// HTTP Clients
builder.Services.AddHttpClient("KeycloakClient");
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

//builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenService, MockTokenService>(); // ← mock for dev

builder.Services.AddScoped<IContractService, ContractService>();

// Error handling
builder.Services.AddProblemDetails();

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