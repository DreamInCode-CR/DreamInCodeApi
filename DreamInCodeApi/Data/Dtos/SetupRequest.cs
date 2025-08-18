using System.ComponentModel.DataAnnotations;

namespace DreamInCodeApi.Data.Dtos;

public class SetupRequest
{
    // Usuario
    [MaxLength(100)] public string? Nombre { get; set; }
    [MaxLength(100)] public string? Apellido1 { get; set; }
    [MaxLength(100)] public string? Apellido2 { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    [MaxLength(30)]  public string? Telefono { get; set; }
    [MaxLength(255)] public string? Direccion { get; set; }

    // Perfil
    public string? Preferencias { get; set; }    
    public string? NotasMedicas { get; set; }
}