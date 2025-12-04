using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class RegistroViewModel
    {
        [Required]
        [EmailAddress]
        public string CorreoElectronico { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; }

        [Required]
        public string Rol { get; set; }
    }
}
