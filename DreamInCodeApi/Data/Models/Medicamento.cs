using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("Medicamentos")]
public class Medicamento
{
    [Key]
    public int MedicamentoID { get; set; }

    [Required]
    public int UsuarioID { get; set; }

    [Required, StringLength(100)]
    public string NombreMedicamento { get; set; } = null!;

    [StringLength(100)]
    public string? Dosis { get; set; }

    [StringLength(255)]
    public string? Instrucciones { get; set; }

    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaHasta { get; set; }

    // Días de la semana
    public bool Lunes { get; set; }
    public bool Martes { get; set; }
    public bool Miercoles { get; set; }
    public bool Jueves { get; set; }
    public bool Viernes { get; set; }
    public bool Sabado { get; set; }
    public bool Domingo { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    
    public TimeSpan? HoraToma { get; set; }
    
    [ForeignKey(nameof(UsuarioID))]
    public virtual Usuarios Usuario { get; set; } = null!;
}