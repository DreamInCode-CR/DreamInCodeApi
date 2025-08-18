using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamInCodeApi.Data.Models;

[Table("Enfermedades")]
public class Enfermedades
{
    [Key]
    public int EnfermedadID { get; set; }

    [Required]
    public string Nombre { get; set; } = null!;
}