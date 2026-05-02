using NotesApp.Api.Models;

namespace NotesApp.Api.Services;

public class NoteService : INoteService
{
    // In-memory store for now — no database yet
    private readonly List<Note> _notes = new();
    private int _nextId = 1;

    public Task<IEnumerable<Note>> GetAllAsync()
    {
        return Task.FromResult(_notes.AsEnumerable());
    }

    public Task<Note?> GetByIdAsync(int id)
    {
        var note = _notes.FirstOrDefault(n => n.Id == id);
        return Task.FromResult(note);
    }

    public Task<Note> CreateAsync(Note note)
    {
        note.Id = _nextId++;
        note.CreatedAt = DateTime.UtcNow;
        _notes.Add(note);
        return Task.FromResult(note);
    }

    public Task<Note?> UpdateAsync(int id, Note note)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null) return Task.FromResult<Note?>(null);

        existing.Title = note.Title;
        existing.Content = note.Content;
        return Task.FromResult<Note?>(existing);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var note = _notes.FirstOrDefault(n => n.Id == id);
        if (note is null) return Task.FromResult(false);

        _notes.Remove(note);
        return Task.FromResult(true);
    }
}