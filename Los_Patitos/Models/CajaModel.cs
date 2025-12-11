
using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class CajaModel : EntidadAuditable //CajaModel hereda campos de auditoría (CreatedAt, ModifiedAt) de EntidadAuditable
    {
        public int IdCaja { get; set; }

        [Required(ErrorMessage = "El comercio asociado es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un comercio válido.")]
        public int IdComercio { get; set; }

        [Required(ErrorMessage = "El comercio asociado es obligatorio.")]
        public Comercio Comercio { get; set; }

        [Required(ErrorMessage = "El nombre de la caja es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9\s]{2,100}$",
            ErrorMessage = "El nombre solo puede contener letras, números y espacios.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(150, ErrorMessage = "La descripción no debe exceder 150 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El teléfono SINPE es obligatorio.")]
        [StringLength(10, ErrorMessage = "El teléfono SINPE no debe exceder 10 caracteres.")]
        [RegularExpression(
            @"^[0-9]{8}$",
            ErrorMessage = "El teléfono SINPE debe contener exactamente 8 dígitos.")]
        public string TelefonoSINPE { get; set; }

        public DateTime FechaDeRegistro { get; set; }

        public DateTime? FechaDeModificacion { get; set; }

        public bool Estado { get; set; }  // true = activo, false = inactivo
    }
}



//-- Tabla Caja_G4
//   CREATE TABLE Caja_G4
//   IdCaja int(11) NOT NULL AUTO_INCREMENT
//   IdComercio int(11) NOT NULL,
//   Nombre varchar(100) NOT NULL,
//   Descripcion varchar(150) NOT NULL,
//   TelefonoSINPE varchar(10) NOT NULL,
//   FechaDeRegistro datetime NOT NULL,
//   FechaDeModificacion datetime DEFAULT NULL,
//   Estado` bit(1) NOT NULL,
//   PRIMARY KEY (`IdCaja`),
//   KEY `FK_CAJAS_COMERCIO_idx` (`IdComercio`),
//   CONSTRAINT `FK_CAJAS_COMERCIO` FOREIGN KEY (`IdComercio`) REFERENCES `comercio_G4` (`IdComercio`) 
//   ON DELETE NO ACTION ON UPDATE NO ACTION ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci'