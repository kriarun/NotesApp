using System.ComponentModel.DataAnnotations;

namespace NotesApp.Api.Options;

public class NotesOptions
{
    [Required]
    [Range(1, 1000)]
    public int MaxNotesPerUser { get; set; }

    public bool AllowEmptyContent { get; set; }

    [Required]
    public string AppDisplayName { get; set; } = string.Empty;
}