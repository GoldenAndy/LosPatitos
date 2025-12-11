using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class TipoComercio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de comercio es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9\s]{2,100}$",
            ErrorMessage = "El nombre solo puede contener letras, números y espacios.")]
        public string Nombre { get; set; } = string.Empty;
    }
}
