namespace Los_Patitos.Models
{
    public class ReporteMensual
    {
        public int IdReporte { get; set; }

        public int IdComercio { get; set; }
        public Comercio? Comercio { get; set; }

        public int CantidadDeCajas { get; set; }
        public decimal MontoTotalRecaudado { get; set; }
        public int CantidadDeSINPES { get; set; }
        public decimal MontoTotalComision { get; set; }
        public DateTime FechaDelReporte { get; set; }
    }
}
