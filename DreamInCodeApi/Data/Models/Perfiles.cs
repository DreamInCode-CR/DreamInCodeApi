using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("Perfiles")]
public class Perfiles
{
    [Key]
    public int PerfilID { get; set; }

    public int UsuarioID { get; set; }

    public string? Idioma { get; set; }
    public string? Voz { get; set; }
    
    public bool Consentimiento { get; set; }   
    public string TamanoTexto { get; set; } = "M";  
    public bool AltoContraste { get; set; }    


    public string? Preferencias { get; set; }

    public string? NotasMedicas { get; set; }
    
    [ForeignKey(nameof(UsuarioID))]
    public virtual Usuarios Usuario { get; set; } = null!;
}