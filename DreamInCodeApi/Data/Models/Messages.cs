using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("Messages")]
public class Messages
{
    [Key]
    public int MessageID { get; set; }

    public int ThreadID { get; set; }

    [Required]
    public string Role { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(ThreadID))]
    public virtual ChatThreads Thread { get; set; } = null!;
}