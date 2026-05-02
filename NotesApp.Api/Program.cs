

using NotesApp.Api.Services;
using NotesApp.Api.Options;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(); // ← register controller support
builder.Services.AddProblemDetails();

// Program.cs
builder.Services.AddOptions<NotesOptions>()
    .Bind(builder.Configuration.GetSection("NotesOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<INoteService, NoteService>(); // ← was AddScoped

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // ← full details in dev only

}else
{
    app.UseExceptionHandler(); // ← clean error in production
}

app.UseStatusCodePages(); // ← handles 404, 405 etc automatically

app.UseHttpsRedirection();
app.MapControllers(); // ← wire controller routes into pipeline

app.Run();
namespace NotesApp.Api
{
    public partial class Program { }
}
