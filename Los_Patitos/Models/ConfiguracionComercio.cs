using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class ConfiguracionComercio 
    {
        public int IdConfiguracion { get; set; }

        [Required]
        public int IdComercio { get; set; }

   
        public Comercio? Comercio { get; set; }

        [Required(ErrorMessage = "El tipo de configuración es obligatorio.")]
        [Range(1, 3, ErrorMessage = "Seleccione un tipo de configuración válido.")]
        public int TipoConfiguracion { get; set; } 

        [Required(ErrorMessage = "La comisión es obligatoria.")]
        [Range(0, 100, ErrorMessage = "La comisión debe estar entre 0 y 100.")]
        public int Comision { get; set; }

        public DateTime FechaDeRegistro { get; set; }
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
