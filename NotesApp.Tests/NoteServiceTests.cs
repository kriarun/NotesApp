using NotesApp.Api.Models;
using NotesApp.Api.Services;

namespace NotesApp.Tests;

public class NoteServiceTests
{
    private readonly NoteService _sut; // sut = System Under Test

    public NoteServiceTests()
    {
        _sut = new NoteService();
    }

    [Fact]
    public async Task GetAllAsync_WhenNoNotes_ReturnsEmptyList()
    {
        // Arrange — nothing to arrange, empty service

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_WhenNoteCreated_ReturnsNoteWithId()
    {
        // Arrange
        var note = new Note { Title = "Test", Content = "Content" };

        // Act
        var result = await _sut.CreateAsync(note);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Title);
    }
}