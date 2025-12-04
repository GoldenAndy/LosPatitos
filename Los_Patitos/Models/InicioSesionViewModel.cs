using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class InicioSesionViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [Display(Name = "Correo electrónico")]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }

        // *** ESTA PROPIEDAD FALTABA ***
        public string? ReturnUrl { get; set; }
    }
}
