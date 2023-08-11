using System.ComponentModel.DataAnnotations;

namespace Palantir.Homatic.Mock;

public class HomaticOptions
{
    [Required]
    public string? RootPath { get; set; }
}
