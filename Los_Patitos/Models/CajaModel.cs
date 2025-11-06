
namespace Los_Patitos.Models
{
    public class CajaModel : EntidadAuditable //CajaModel hereda campos de auditoría (CreatedAt, ModifiedAt) de EntidadAuditable
    {
        public int IdCaja { get; set; }

        public int IdComercio { get; set; }
        public Comercio Comercio { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

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