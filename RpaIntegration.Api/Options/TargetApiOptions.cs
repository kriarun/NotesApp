using System.ComponentModel.DataAnnotations;

namespace RpaIntegration.Api.Options;

public class TargetApiOptions
{
    [Required] public string BaseUrl { get; set; } = string.Empty;
}