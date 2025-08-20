using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("ReminderLog")]
public class ReminderLog
{
    [Key]
    public int ReminderLogID { get; set; }

    [Required]
    public int UsuarioID { get; set; }

    [Required]
    public int MedicamentoID { get; set; }

    [Required]
    public DateTime FechaProgramada { get; set; } 

    [Required]
    public TimeSpan HoraProgramada { get; set; }  

    public DateTime DisparadoAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UsuarioID))]
    public virtual Usuarios Usuario { get; set; } = null!;

    [ForeignKey(nameof(MedicamentoID))]
    public virtual Medicamento Medicamento { get; set; } = null!;
}