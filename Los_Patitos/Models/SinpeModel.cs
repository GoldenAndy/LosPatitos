

using Microsoft.AspNetCore.Http.HttpResults;

namespace Los_Patitos.Models
{
    public class SinpeModel : EntidadAuditable //SinpeModel hereda campos de auditoría (CreatedAt, ModifiedAt) de EntidadAuditable
    {
        public int IdSinpe { get; set; }

        public string TelefonoOrigen { get; set; }

        public string NombreOrigen { get; set; }

        public string TelefonoDestinaria { get; set; }

        public string NombreDestinaria { get; set; }

        public decimal Monto { get; set; }
        public DateTime FechaDeRegistro { get; set; } = DateTime.Now;

        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = false;



        // Relación con la caja que recibió el SINPE
        public int IdCaja { get; set; }
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