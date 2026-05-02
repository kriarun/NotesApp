using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NotesApp.Api.Models;

namespace NotesApp.Tests;

public class NotesIntegrationTests : IClassFixture<WebApplicationFactory<NotesApp.Api.Program>>
{
    private readonly WebApplicationFactory<NotesApp.Api.Program> _factory;

    private readonly HttpClient _client;

    public NotesIntegrationTests()
    {
        _factory = new WebApplicationFactory<NotesApp.Api.Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/notes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
        Assert.Empty(notes);
    }

    [Fact]
    public async Task CreateNote_ReturnsCreated()
    {
        // Arrange
        var note = new Note { Title = "Integration Test Note", Content = "Test Content" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/notes", note);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(created);
        Assert.Equal("Integration Test Note", created.Title);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetById_WhenNoteNotFound_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/notes/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}