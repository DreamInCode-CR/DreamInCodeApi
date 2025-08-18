using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("ChatThreads")]
public class ChatThreads
{
    [Key]
    public int ThreadID { get; set; }

    public int UsuarioID { get; set; }

    public string? Titulo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Messages> Messages { get; set; } = new List<Messages>();

    [ForeignKey(nameof(UsuarioID))]
    public virtual Usuarios Usuario { get; set; } = null!;
}