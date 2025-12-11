
using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class UsuarioModel : EntidadAuditable  //hereda campos de auditoría (CreatedAt, ModifiedAt) de EntidadAuditable
    {
        public int IdUsuario { get; set; }

        [StringLength(36, ErrorMessage = "El IdNetUser no debe exceder 36 caracteres.")]
        public string? IdNetUser { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]{2,100}$",
            ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El primer apellido no debe exceder 100 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]{2,100}$",
            ErrorMessage = "El primer apellido solo puede contener letras y espacios.")]
        public string PrimerApellido { get; set; }

        [Required(ErrorMessage = "El segundo apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El segundo apellido no debe exceder 100 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]{2,100}$",
            ErrorMessage = "El segundo apellido solo puede contener letras y espacios.")]
        public string SegundoApellido { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        [StringLength(9, ErrorMessage = "La identificación debe tener exactamente 9 dígitos.")]
        [RegularExpression(
        @"^[0-9]{9}$",
        ErrorMessage = "La identificación debe contener exactamente 9 dígitos.")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [StringLength(100, ErrorMessage = "El correo no debe exceder 100 caracteres.")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
        public string CorreoElectronico { get; set; }  
        public DateTime FechaDeRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; } = true;

        [Required(ErrorMessage = "El comercio asociado es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un comercio válido.")]
        public int IdComercio { get; set; }

        [Required]
        public Comercio Comercio { get; set; }
    }
}

//CREATE TABLE `u484426513_quta`.`Usuario_G4` (
//  `IdUsuario` INT NOT NULL,
//  `Nombres` VARCHAR(100) NOT NULL,
//  `PrimerApellido` VARCHAR(100) NOT NULL,
//  `SegundoApellido` VARCHAR(100) NOT NULL,
//  `Identificacion` VARCHAR(10) NOT NULL,
//  `CorreoElectronico` VARCHAR(200) NOT NULL,
//  `FechaDeRegistro` DATETIME NOT NULL,
//  `FechaDeModificacion` DATETIME NULL,
//  `Estado` BIT NOT NULL,
//  `IdComercio` INT NOT NULL,
//  `IdNetUser` CHAR(36) NULL,
//  PRIMARY KEY (`IdUsuario`),
//  UNIQUE INDEX `UX_Usuario_Identificacion` (`Identificacion` ASC),
//  `FK_USUARIO_COMERCIO_idx` (`IdComercio` ASC) VISIBLE,
//  CONSTRAINT `FK_USUARIO_COMERCIO`
//  FOREIGN KEY (`IdComercio`) REFERENCES `u484426513_quta`.`Comercio_G4` (`IdComercio`)
//  ON DELETE NO ACTION ON UPDATE NO ACTION
//);
