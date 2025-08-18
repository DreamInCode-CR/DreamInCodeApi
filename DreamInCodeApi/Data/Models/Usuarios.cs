using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("Usuarios")]
public class Usuarios
{
    [Key]
    public int UsuarioID { get; set; }

    public string? Nombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Observaciones { get; set; }

    public DateTime FechaRegistro { get; set; }
    public int TipoUsuario { get; set; }

    [Required]
    public string Correo { get; set; } = null!;

    public string? PasswordHash { get; set; }
    public DateTime? PasswordUpdatedAt { get; set; }
}