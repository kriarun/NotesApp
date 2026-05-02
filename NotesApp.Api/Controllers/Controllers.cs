using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NotesApp.Api.Models;
using NotesApp.Api.Services;
using NotesApp.Api.Options;

namespace NotesApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")] // ← add this

public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly NotesOptions _options;

    public NotesController(INoteService noteService, IOptions<NotesOptions> options)
    {
        _noteService = noteService;
        _options = options.Value;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Note>>> GetAll()
    {
        var notes = await _noteService.GetAllAsync();
        
        Response.Headers["X-App-Name"] = _options.AppDisplayName;
            
        return Ok(notes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetById(int id)
    {
        var note = await _noteService.GetByIdAsync(id);
        if (note is null) return NotFound();
        return Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<Note>> Create(Note note)
    {
        var created = await _noteService.CreateAsync(note);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Note>> Update(int id, Note note)
    {
        var updated = await _noteService.UpdateAsync(id, note);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _noteService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}