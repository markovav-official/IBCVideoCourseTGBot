using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBCVideoCourseTGBot.Models;

[Table("users")]
public class User
{
    [Key] public long Id { get; set; }

    [MaxLength(32)] public string? Alias { get; set; }

    [MaxLength(128)] public string Email { get; set; } = null!;

    public bool Confirmed { get; set; } = false;
    public bool Finished { get; set; } = false;
}