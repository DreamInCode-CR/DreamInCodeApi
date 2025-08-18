using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("RespuestasPersonalizadas")]
public class RespuestasPersonalizadas
{
    [Key]
    public int RespuestaID { get; set; }

    public int? UsuarioID { get; set; }

    public string? Pregunta { get; set; }
    public string? Respuesta { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    [ForeignKey(nameof(UsuarioID))]
    public virtual Usuarios? Usuario { get; set; }
}

