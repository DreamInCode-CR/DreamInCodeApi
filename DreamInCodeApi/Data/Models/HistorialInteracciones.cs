using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("HistorialInteracciones")]
public class HistorialInteracciones
{
    [Key]
    public int InteraccionID { get; set; }

    public int? UsuarioID { get; set; }

    public string? EntradaVozTexto { get; set; }
    public string? RespuestaGenerada { get; set; }
    public DateTime? Fecha { get; set; }

    [ForeignKey(nameof(UsuarioID))]
    public virtual Usuarios? Usuario { get; set; }
}