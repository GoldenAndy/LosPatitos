
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Los_Patitos.Models
{
    [Table("BITACORA_EVENTOS")]
    public class BitacoraEvento {
        [Key] public int IdEvento { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Para que el Id lo genere la BD de forma auto incremental
        [Required, MaxLength(20)] public string TablaDeEvento { get; set; } = null!;
        [Required, MaxLength(20)] public string TipoDeEvento { get; set; } = null!; 
        [Required] public DateTime FechaDeEvento { get; set; } = DateTime.Now;
        [Required] public string DescripcionDeEvento { get; set; } = string.Empty;
        [Required] public string StackTrace { get; set; } = string.Empty;
        public string? DatosAnteriores { get; set; }   // JSON
        public string? DatosPosteriores { get; set; }  // JSON
    }
}