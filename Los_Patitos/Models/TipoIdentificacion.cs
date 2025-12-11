using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class TipoIdentificacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de identificación es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no debe exceder 50 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]{2,50}$",
            ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;
    }
}