using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class Comercio : EntidadAuditable //Comercio hereda campos de auditoría (CreatedAt, ModifiedAt) de EntidadAuditable
    {
        public int IdComercio { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        [StringLength(30, ErrorMessage = "La identificación no debe exceder 30 caracteres.")]
        public string Identificacion { get; set; } = string.Empty;

        [Range(1, 2, ErrorMessage = "Seleccione un tipo de identificación válido.")]
        public int TipoIdentificacion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no debe exceder 200 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]{2,200}$",
            ErrorMessage = "El nombre solo puede contener letras y espacios."
        )]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Seleccione un tipo de comercio válido.")]
        public int TipoComercio { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(20, ErrorMessage = "El teléfono no debe exceder 20 caracteres.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener exactamente 8 dígitos.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
        [StringLength(200, ErrorMessage = "El correo no debe exceder 200 caracteres.")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(500, ErrorMessage = "La dirección no debe exceder 500 caracteres.")]
        public string Direccion { get; set; } = string.Empty;

        public DateTime FechaDeRegistro { get; set; }
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
