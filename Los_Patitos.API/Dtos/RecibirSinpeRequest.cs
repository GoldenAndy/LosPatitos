using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.API.Dtos
{
    public class RecibirSinpeRequest
    {
        [Required]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El teléfono de origen debe tener 8 dígitos.")]
        public string TelefonoOrigen { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string NombreOrigen { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El teléfono destino debe tener 8 dígitos.")]
        public string TelefonoDestinatario { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string NombreDestinatario { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999999999.99, ErrorMessage = "El monto debe ser mayor a 0.")]
        public decimal Monto { get; set; }

        [StringLength(50)]
        public string? Descripcion { get; set; }
    }
}
