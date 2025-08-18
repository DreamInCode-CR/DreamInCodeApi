using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("PreferenciasUsuario")]
public class PreferenciasUsuario
{
    [Key]
    public int PreferenciaID { get; set; }

    public int? UsuarioID { get; set; }

    public string? TipoPreferencia { get; set; }
    public string? Valor { get; set; }

    [ForeignKey(nameof(UsuarioID))]
    public virtual Usuarios? Usuario { get; set; }
}