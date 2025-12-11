

using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace Los_Patitos.Models
{
    public class SinpeModel : EntidadAuditable //SinpeModel hereda campos de auditoría (CreatedAt, ModifiedAt) de EntidadAuditable
    {
        public int IdSinpe { get; set; }

        [Required(ErrorMessage = "El teléfono de origen es obligatorio.")]
        [StringLength(10, ErrorMessage = "El teléfono de origen no debe exceder 10 caracteres.")]
        [RegularExpression(@"^[0-9]{8}$",
            ErrorMessage = "El teléfono de origen debe contener exactamente 8 dígitos.")]
        public string TelefonoOrigen { get; set; }

        [Required(ErrorMessage = "El nombre de origen es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre de origen no debe exceder 200 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]{2,200}$",
            ErrorMessage = "El nombre de origen solo puede contener letras y espacios.")]
        public string NombreOrigen { get; set; }

        [Required(ErrorMessage = "El teléfono destino es obligatorio.")]
        [StringLength(10, ErrorMessage = "El teléfono destino no debe exceder 10 caracteres.")]
        [RegularExpression(@"^[0-9]{8}$",
            ErrorMessage = "El teléfono destino debe contener exactamente 8 dígitos.")]
        public string TelefonoDestinaria { get; set; }

        [Required(ErrorMessage = "El nombre del receptor es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre del receptor no debe exceder 200 caracteres.")]
        [RegularExpression(
            @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]{2,200}$",
            ErrorMessage = "El nombre del receptor solo puede contener letras y espacios.")]
        public string NombreDestinaria { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, 999999999999999.99, ErrorMessage = "El monto debe ser mayor a 0.")]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }
        public DateTime FechaDeRegistro { get; set; } = DateTime.Now;

        [StringLength(50, ErrorMessage = "La descripción no debe exceder 50 caracteres.")]
        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = false;



        // Relación con la caja que recibió el SINPE
        [Required(ErrorMessage = "La caja asociada es obligatoria.")]
        public int IdCaja { get; set; }

        [Required(ErrorMessage = "La caja asociada es obligatoria.")]
        public CajaModel Caja { get; set; }
    }
}




//CREATE TABLE `u484426513_quta`.`Sinpe_G4` (
//  `IdSinpe` INT NOT NULL,
//  `TelefonoOrigen` VARCHAR(10) NOT NULL,
//  `NombreOrigen` VARCHAR(200) NOT NULL,
//  `TelefonoDestinaria` VARCHAR(10) NOT NULL,
//  `NombreDestinaria` VARCHAR(200) NOT NULL,
//  `Monto` DECIMAL(18,2) NOT NULL,
//  `FechaDeRegistro` DATETIME NOT NULL,
//  `Descripcion` VARCHAR(50) NULL,
//  `Estado` BIT NOT NULL,
//  `IdCaja` INT NOT NULL,
// PRIMARY KEY (`IdSinpe`),
// FK_SINPE_CAJA_idx` (`IdCaja` ASC) VISIBLE,CONSTRAINT `FK_SINPE_CAJA`
// FOREIGN KEY (`IdCaja`) REFERENCES `u484426513_quta`.`Caja_G4` (`IdCaja`)
// ON DELETE NO ACTION ON UPDATE NO ACTION);