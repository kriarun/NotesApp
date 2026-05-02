using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NotesApp.Api.Controllers;
using NotesApp.Api.Models;
using NotesApp.Api.Options;
using NotesApp.Api.Services;

namespace NotesApp.Tests;

public class NotesControllerTests
{
    private readonly Mock<INoteService> _mockService;
    private readonly NotesController _sut;

    public NotesControllerTests()
    {
        _mockService = new Mock<INoteService>();
        
        var options = Options.Create(new NotesOptions
        {
            AppDisplayName = "Test App",
            MaxNotesPerUser = 100,
            AllowEmptyContent = false
        });

        _sut = new NotesController(_mockService.Object, options);
        
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithNotes()
    {
        // Arrange
        var notes = new List<Note>
        {
            new Note { Id = 1, Title = "Note 1" },
            new Note { Id = 2, Title = "Note 2" }
        };

        _mockService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(notes);

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedNotes = Assert.IsAssignableFrom<IEnumerable<Note>>(okResult.Value);
        Assert.Equal(2, returnedNotes.Count());
    }

    [Fact]
    public async Task GetById_WhenNoteExists_ReturnsOk()
    {
        // Arrange
        var note = new Note { Id = 1, Title = "Note 1" };
        _mockService
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(note);

        // Act
        var result = await _sut.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedNote = Assert.IsType<Note>(okResult.Value);
        Assert.Equal(1, returnedNote.Id);
    }

    [Fact]
    public async Task GetById_WhenNoteNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockService
            .Setup(s => s.GetByIdAsync(99))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await _sut.GetById(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task GetAll_SetsAppNameHeader()
    {
        // Arrange
        _mockService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<Note>());

        // Act
        await _sut.GetAll();

        // Assert
        var header = _sut.Response.Headers["X-App-Name"].ToString();
        Assert.Equal("Test App", header);
    }
}