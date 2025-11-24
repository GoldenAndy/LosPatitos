
namespace Los_Patitos.Models
{
    public class UsuarioModel : EntidadAuditable  //hereda campos de auditoría (CreatedAt, ModifiedAt) de EntidadAuditable
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string Identificacion { get; set; }
        public string CorreoElectronico { get; set; }


        public DateTime FechaDeRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaDeModificacion { get; set; }
        public bool Estado { get; set; } = true;

        public int IdComercio { get; set; }
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
